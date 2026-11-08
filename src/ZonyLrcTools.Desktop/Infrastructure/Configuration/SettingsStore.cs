using System.Globalization;
using System.Text;
using Microsoft.Extensions.Options;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using ZonyLrcTools.Common.Configuration;

namespace ZonyLrcTools.Desktop.Infrastructure.Configuration;

/// <inheritdoc cref="ISettingsStore" />
/// <remarks>
/// <para>
/// Saving must not destroy the hand-written comments in <c>config.yaml</c>: that file is the
/// documented configuration surface of this project, and a full re-serialize flattens it into an
/// unreadable machine blob on the very first checkbox click.
/// </para>
/// <para>
/// So the store patches the values it owns in place, line by line, and only falls back to a full
/// re-serialize when the on-disk text no longer round-trips to the intended values. The patched
/// text is parsed back and compared before it is committed, so a bad patch can never overwrite a
/// working configuration.
/// </para>
/// </remarks>
public sealed class SettingsStore : ISettingsStore, IDisposable
{
    private const string RegeneratedHeaderText =
        "# 此文件由 ZonyLrcTools X 重新生成：原有的注释已丢失，你可以参照文档重新补充。";

    private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private bool _disposed;

    public SettingsStore(IOptions<GlobalOptions> options)
    {
        Options = options.Value;

        _serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            // Keep "\n" readable instead of letting YamlDotNet emit the folded block scalar ">2".
            .WithEventEmitter(next => new QuotedScalarEventEmitter(next))
            .Build();

        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    /// <inheritdoc />
    public GlobalOptions Options { get; }

    /// <inheritdoc />
    public string ConfigFilePath => AppPaths.ConfigFilePath;

    /// <inheritdoc />
    public async Task<SettingsSaveResult> SaveAsync(CancellationToken cancellationToken = default)
    {
        await _writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var targetPath = ConfigFilePath;
            var managed = BuildManagedScalars(Options);

            var text = await TryPatchInPlaceAsync(targetPath, managed, cancellationToken).ConfigureAwait(false)
                       ?? BuildFromScratch();

            var directory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Write to a sibling temp file first so a crash mid-write cannot truncate the
            // settings the user already had.
            var tempPath = targetPath + ".tmp";
            await File.WriteAllTextAsync(tempPath, text, new UTF8Encoding(false), cancellationToken)
                .ConfigureAwait(false);
            File.Move(tempPath, targetPath, overwrite: true);

            return SettingsSaveResult.Success(targetPath);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return SettingsSaveResult.Failure(ex.Message);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    /// <summary>
    /// Rewrites only the managed scalar values in place, keeping every comment and blank line.
    /// Returns <c>null</c> when the file is missing or the patched text does not read back as the
    /// intended configuration.
    /// </summary>
    /// <remarks>
    /// A managed key does not need its own line to be "correct": <c>config.yaml</c> ships with
    /// <c># depth: 10</c> commented out for QQ, and the default it falls back to is exactly the
    /// value the GUI wants. The round-trip check below is therefore the only gate that matters.
    /// </remarks>
    private async Task<string?> TryPatchInPlaceAsync(
        string path,
        IReadOnlyDictionary<string, string> managed,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        var original = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(original))
        {
            return null;
        }

        var patched = PatchText(original, managed);
        return VerifyRoundTrip(patched, managed) ? patched : null;
    }

    private string BuildFromScratch()
    {
        // Materialize the same managed values the patch path would have written, so the two
        // paths cannot disagree about what "saved" means.
        // YamlDotNet 输出统一是 "\n"，这里显式拼同一套行尾，避免首行注释与正文混用 CRLF/LF。
        var body = _serializer.Serialize(Options);
        return RegeneratedHeaderText + "\n" + body;
    }

    private bool VerifyRoundTrip(string yaml, IReadOnlyDictionary<string, string> managed)
    {
        try
        {
            var parsed = _deserializer.Deserialize<GlobalOptions>(yaml);
            if (parsed == null)
            {
                return false;
            }

            foreach (var (key, expected) in managed)
            {
                if (!string.Equals(ReadScalar(parsed, key), expected, StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Reads the value of a managed path back out of a deserialized options instance.</summary>
    private static string? ReadScalar(GlobalOptions options, string path)
    {
        if (path.StartsWith("provider.lyric.plugin[", StringComparison.Ordinal))
        {
            var close = path.IndexOf(']');
            if (close < 0)
            {
                return null;
            }

            var name = path["provider.lyric.plugin[".Length..close];
            var leaf = path[(close + 2)..];
            var plugin = options.Provider?.Lyric?.Plugin?.FirstOrDefault(p => p?.Name == name);
            if (plugin == null)
            {
                return null;
            }

            return leaf switch
            {
                "priority" => plugin.Priority.ToString(CultureInfo.InvariantCulture),
                "depth" => plugin.Depth.ToString(CultureInfo.InvariantCulture),
                "additional.isEnableRomanOutput" =>
                    plugin.Additional != null && plugin.Additional.TryGetValue("isEnableRomanOutput", out var v)
                        ? Normalize(v)
                        : null,
                _ => null
            };
        }

        if (path.StartsWith("provider.tag.plugin[", StringComparison.Ordinal))
        {
            var close = path.IndexOf(']');
            if (close < 0)
            {
                return null;
            }

            var name = path["provider.tag.plugin[".Length..close];
            var leaf = path[(close + 2)..];
            var plugin = options.Provider?.Tag?.Plugin?.FirstOrDefault(p => p?.Name == name);
            if (plugin == null)
            {
                return null;
            }

            return leaf == "priority" ? plugin.Priority.ToString(CultureInfo.InvariantCulture) : null;
        }

        return path switch
        {
            "networkOptions.isEnable" => Normalize(options.NetworkOptions?.IsEnable),
            "networkOptions.ip" => options.NetworkOptions?.Ip,
            "networkOptions.port" => options.NetworkOptions?.Port.ToString(CultureInfo.InvariantCulture),
            "provider.lyric.config.isOneLine" => Normalize(options.Provider?.Lyric?.Config?.IsOneLine),
            // Line breaks must not be trimmed: "\n" and "" are different configurations.
            "provider.lyric.config.lineBreak" => options.Provider?.Lyric?.Config?.LineBreak,
            "provider.lyric.config.isEnableTranslation" => Normalize(options.Provider?.Lyric?.Config?.IsEnableTranslation),
            "provider.lyric.config.isOnlyOutputTranslation" => Normalize(options.Provider?.Lyric?.Config?.IsOnlyOutputTranslation),
            "provider.lyric.config.isSkipExistLyricFiles" => Normalize(options.Provider?.Lyric?.Config?.IsSkipExistLyricFiles),
            "provider.lyric.config.fileEncoding" => options.Provider?.Lyric?.Config?.FileEncoding,
            "provider.tag.blockWord.isEnable" => Normalize(options.Provider?.Tag?.BlockWord?.IsEnable),
            "provider.tag.blockWord.filePath" => options.Provider?.Tag?.BlockWord?.FilePath,
            _ => null
        };
    }

    /// <summary>Canonical text for a boolean, so "True" and "true" compare equal.</summary>
    private static string Normalize(bool value) => value ? "true" : "false";

    private static string? Normalize(bool? value) => value.HasValue ? Normalize(value.Value) : null;

    private static string Normalize(string? raw) => raw switch
    {
        null => string.Empty,
        "True" => "true",
        "False" => "false",
        _ => raw
    };

    /// <summary>
    /// Walks the YAML text, tracking the current key path by indentation, and replaces only the
    /// value part of lines whose full path is managed. Trailing comments are preserved verbatim.
    /// </summary>
    private static string PatchText(string original, IReadOnlyDictionary<string, string> managed)
    {
        var newline = original.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        var lines = original.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        var stack = new List<(int Indent, string Path)>();
        var replaced = new HashSet<string>(StringComparer.Ordinal);

        for (var i = 0; i < lines.Length; i++)
        {
            var raw = lines[i];
            var trimmed = raw.TrimStart();
            if (trimmed.Length == 0 || trimmed.StartsWith('#'))
            {
                continue;
            }

            var indent = raw.Length - trimmed.Length;

            // Leave the previous sibling scope as soon as the indentation goes back out.
            while (stack.Count > 0 && stack[^1].Indent >= indent)
            {
                stack.RemoveAt(stack.Count - 1);
            }

            var parent = stack.Count > 0 ? stack[^1].Path : string.Empty;

            // List element header, e.g. "      - name: NetEase   # 基于网易云的歌词下载器。"
            if (trimmed.StartsWith("- name:", StringComparison.Ordinal))
            {
                var name = ReadToken(trimmed["- name:".Length..]);
                if (name.Length > 0)
                {
                    stack.Add((indent, $"{parent}[{name}]"));
                }

                continue;
            }

            var colon = trimmed.IndexOf(':');
            if (colon <= 0)
            {
                continue;
            }

            var key = trimmed[..colon].Trim();
            if (key.Length == 0 || key.Contains(' ') || key.StartsWith('-'))
            {
                continue;
            }

            var rest = trimmed[(colon + 1)..];
            var valuePart = ReadToken(rest);
            var fullPath = parent.Length == 0 ? key : $"{parent}.{key}";

            if (valuePart.Length == 0)
            {
                // Section header: children follow at a deeper indentation.
                stack.Add((indent, fullPath));
                continue;
            }

            if (!managed.TryGetValue(fullPath, out var newValue))
            {
                continue;
            }

            if (!replaced.Add(fullPath))
            {
                // 同一个键出现多次时只改第一处：改全部会让 YAML 里前一处生效、后一处变成
                // 死键的语义更难理解，也让回读校验失去意义。留个注释说明这是有意为之。
                continue;
            }

            // Preserve the exact column layout: the value span, then its trailing comment.
            // config.yaml aligns its comments into columns, so a value that grew or shrank must
            // absorb the difference in the padding rather than push the comment sideways.
            var leadingSpaces = rest.Length - rest.TrimStart().Length;
            var valueStart = indent + colon + 1 + leadingSpaces;
            var comment = rest[(leadingSpaces + valuePart.Length)..];
            var replacement = FormatScalar(newValue, valuePart);

            if (comment.Length > 1 && comment[0] == ' ')
            {
                var commentColumn = valueStart + valuePart.Length + (comment.Length - comment.TrimStart().Length);
                var padding = commentColumn - (valueStart + replacement.Length);
                if (padding >= 1)
                {
                    comment = new string(' ', padding) + comment.TrimStart();
                }
            }

            lines[i] = raw[..valueStart] + replacement + comment;
        }

        return string.Join(newline, lines);
    }

    /// <summary>Reads the first YAML token, stopping at whitespace outside quotes.</summary>
    private static string ReadToken(string text)
    {
        var s = text.TrimStart();
        if (s.Length == 0)
        {
            return string.Empty;
        }

        if (s[0] is '"' or '\'')
        {
            var quote = s[0];
            for (var i = 1; i < s.Length; i++)
            {
                if (s[i] == '\\' && quote == '"')
                {
                    i++;
                    continue;
                }

                if (s[i] == quote)
                {
                    return s[..(i + 1)];
                }
            }

            return s;
        }

        var end = 0;
        while (end < s.Length && !char.IsWhiteSpace(s[end]))
        {
            end++;
        }

        return s[..end];
    }

    /// <summary>
    /// Emits the new scalar in the shape the old one had: same quote character when the previous
    /// value was quoted, double quotes when the value contains characters a plain scalar cannot
    /// carry (the line-break value), plain otherwise.
    /// </summary>
    /// <remarks>
    /// A value that merely looks like a boolean is left unquoted: "true" and "false" reach this
    /// method both from boolean fields (where quoting would turn them into strings) and from
    /// string fields (whose original spelling in the file was already quoted). Mirroring the
    /// previous token's quoting therefore keeps both cases correct.
    /// </remarks>
    private static string FormatScalar(string value, string previous)
    {
        var mustQuote = value.Length == 0
                        || value.Any(c => c is '\n' or '\r' or '\t' or '"' or '\\')
                        || value != value.Trim();

        if (!mustQuote)
        {
            if (previous.StartsWith('\'') && !value.Contains('\''))
            {
                return $"'{value}'";
            }

            if (previous.StartsWith('"'))
            {
                return $"\"{value}\"";
            }

            return value;
        }

        var escaped = value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal)
            .Replace("\r", "\\r", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal)
            .Replace("\t", "\\t", StringComparison.Ordinal);

        return $"\"{escaped}\"";
    }

    /// <summary>Flattens the editable subset of <see cref="GlobalOptions"/> into path → scalar text.</summary>
    private static Dictionary<string, string> BuildManagedScalars(GlobalOptions options)
    {
        var managed = new Dictionary<string, string>(StringComparer.Ordinal);

        if (options.NetworkOptions != null)
        {
            managed["networkOptions.isEnable"] = Normalize(options.NetworkOptions.IsEnable);
            managed["networkOptions.ip"] = options.NetworkOptions.Ip ?? string.Empty;
            managed["networkOptions.port"] = options.NetworkOptions.Port.ToString(CultureInfo.InvariantCulture);
        }

        var config = options.Provider?.Lyric?.Config;
        if (config != null)
        {
            managed["provider.lyric.config.isOneLine"] = Normalize(config.IsOneLine);
            managed["provider.lyric.config.lineBreak"] = config.LineBreak ?? string.Empty;
            managed["provider.lyric.config.isEnableTranslation"] = Normalize(config.IsEnableTranslation);
            managed["provider.lyric.config.isOnlyOutputTranslation"] = Normalize(config.IsOnlyOutputTranslation);
            managed["provider.lyric.config.isSkipExistLyricFiles"] = Normalize(config.IsSkipExistLyricFiles);
            managed["provider.lyric.config.fileEncoding"] = config.FileEncoding ?? string.Empty;
        }

        var blockWord = options.Provider?.Tag?.BlockWord;
        if (blockWord != null)
        {
            managed["provider.tag.blockWord.isEnable"] = Normalize(blockWord.IsEnable);
            managed["provider.tag.blockWord.filePath"] = blockWord.FilePath ?? string.Empty;
        }

        foreach (var plugin in options.Provider?.Lyric?.Plugin ?? [])
        {
            if (plugin?.Name == null)
            {
                continue;
            }

            var prefix = $"provider.lyric.plugin[{plugin.Name}]";
            managed[$"{prefix}.priority"] = plugin.Priority.ToString(CultureInfo.InvariantCulture);
            managed[$"{prefix}.depth"] = plugin.Depth.ToString(CultureInfo.InvariantCulture);

            if (plugin.Additional != null &&
                plugin.Additional.TryGetValue("isEnableRomanOutput", out var roman))
            {
                managed[$"{prefix}.additional.isEnableRomanOutput"] = Normalize(roman);
            }
        }

        foreach (var plugin in options.Provider?.Tag?.Plugin ?? [])
        {
            if (plugin?.Name == null)
            {
                continue;
            }

            managed[$"provider.tag.plugin[{plugin.Name}].priority"] = plugin.Priority.ToString(CultureInfo.InvariantCulture);
        }

        return managed;
    }

    /// <summary>
    /// Releases the write lock. <see cref="SettingsStore" /> is a singleton, so this only runs at
    /// shutdown, but the lock is a disposable handle and leaving it to finalization is sloppy.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _writeLock.Dispose();
    }
}
