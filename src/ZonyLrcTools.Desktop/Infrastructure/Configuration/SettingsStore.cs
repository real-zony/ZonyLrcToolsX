using System.Globalization;
using System.Text;
using Microsoft.Extensions.Options;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using ZonyLrcTools.Common.Configuration;

namespace ZonyLrcTools.Desktop.Infrastructure.Configuration;

/// <inheritdoc cref="ISettingsStore" />
/// <remarks>只修改界面管理的值，保留其他配置和注释；无法安全修改时返回错误。</remarks>
public sealed class SettingsStore : ISettingsStore, IDisposable
{
    private const string RegeneratedHeaderText = "# ZonyLrcTools X 配置文件";

    private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private bool _disposed;

    /// <summary>使用应用共享配置创建设置存储。</summary>
    public SettingsStore(IOptions<GlobalOptions> options)
    {
        Options = options.Value;

        _serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            // 换行符保持双引号转义形式，便于手工编辑。
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

            // 使用独立临时文件，避免多个进程互相截断写入。
            var tempPath = targetPath + $".{Guid.NewGuid():N}.tmp";
            try
            {
                await File.WriteAllTextAsync(tempPath, text, new UTF8Encoding(false), cancellationToken)
                    .ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                File.Move(tempPath, targetPath, overwrite: true);
            }
            finally
            {
                File.Delete(tempPath);
            }

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

    /// <summary>按 YAML 节点位置修改配置；仅文件不存在时允许重新生成。</summary>
    private async Task<string?> TryPatchInPlaceAsync(
        string path,
        IReadOnlyDictionary<string, string> managed,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        var original = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
        var patched = PatchText(original, managed);
        if (!VerifyRoundTrip(patched, managed))
        {
            throw new InvalidDataException("无法安全修改配置文件，请检查 YAML 内容。原文件未更改。");
        }

        return patched;
    }

    private string BuildFromScratch()
    {
        // 新文件使用统一的 LF 行尾。
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

    /// <summary>读取反序列化后的受管配置值。</summary>
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
            // 换行符不能去除空白。
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

    /// <summary>统一布尔值的文本形式。</summary>
    private static string Normalize(bool value) => value ? "true" : "false";

    private static string? Normalize(bool? value) => value.HasValue ? Normalize(value.Value) : null;

    private static string Normalize(string? raw) => raw switch
    {
        null => string.Empty,
        "True" => "true",
        "False" => "false",
        _ => raw
    };

    /// <summary>用解析器的位置替换标量，保留流式映射、注释和未管理的字段。</summary>
    private static string PatchText(string original, IReadOnlyDictionary<string, string> managed)
    {
        var stream = new YamlStream();
        stream.Load(new StringReader(original));
        if (stream.Documents.Count != 1 || stream.Documents[0].RootNode is not YamlMappingNode root)
        {
            throw new InvalidDataException("配置必须是单个 YAML 映射文档。");
        }

        var edits = new List<(int Start, int End, string Text)>();
        var additions = new Dictionary<YamlMappingNode, Dictionary<string, object>>(ReferenceEqualityComparer.Instance);
        foreach (var (path, value) in managed)
        {
            var parts = path.Split('.');
            var current = (YamlNode)root;
            for (var i = 0; i < parts.Length; i++)
            {
                if (!current.Anchor.IsEmpty || current is not YamlMappingNode mapping)
                {
                    throw new InvalidDataException($"无法安全修改带锚点或非映射的配置：{path}");
                }

                var part = parts[i];
                var bracket = part.IndexOf('[');
                var key = bracket < 0 ? part : part[..bracket];
                if (!mapping.Children.TryGetValue(new YamlScalarNode(key), out var child))
                {
                    if (bracket >= 0)
                    {
                        throw new InvalidDataException($"配置源已更改，请重新启动应用：{path}");
                    }

                    if (!additions.TryGetValue(mapping, out var fields))
                    {
                        fields = new Dictionary<string, object>(StringComparer.Ordinal);
                        additions.Add(mapping, fields);
                    }

                    for (var j = i; j < parts.Length - 1; j++)
                    {
                        if (parts[j].Contains('['))
                        {
                            throw new InvalidDataException($"配置源已更改，请重新启动应用：{path}");
                        }
                        if (!fields.TryGetValue(parts[j], out var nested))
                        {
                            nested = new Dictionary<string, object>(StringComparer.Ordinal);
                            fields.Add(parts[j], nested);
                        }
                        fields = (Dictionary<string, object>)nested;
                    }
                    fields[parts[^1]] = value;
                    break;
                }

                if (bracket >= 0)
                {
                    var name = part[(bracket + 1)..^1];
                    child = (child as YamlSequenceNode)?.Children.OfType<YamlMappingNode>()
                        .SingleOrDefault(item => item.Children.TryGetValue(new YamlScalarNode("name"), out var node)
                            && node is YamlScalarNode scalar && scalar.Value == name)
                        ?? throw new InvalidDataException($"找不到配置源：{name}");
                }

                current = child;
                if (i != parts.Length - 1)
                {
                    continue;
                }
                if (child is not YamlScalarNode existing || !child.Anchor.IsEmpty)
                {
                    throw new InvalidDataException($"无法安全修改非标量或带锚点的配置：{path}");
                }
                if (existing.Value == value)
                {
                    continue;
                }
                if (existing.Style is ScalarStyle.Literal or ScalarStyle.Folded)
                {
                    throw new InvalidDataException($"请先将多行配置值改为双引号形式：{path}。原文件未更改。");
                }

                edits.Add(((int)child.Start.Index, (int)child.End.Index, QuoteScalar(value)));
            }
        }

        var newline = original.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        foreach (var (mapping, fields) in additions)
        {
            if (mapping.Style == MappingStyle.Flow)
            {
                // 在首个键之前插入；空映射则插入到左花括号之后。
                var position = mapping.Children.Count > 0
                    ? (int)mapping.Children.First().Key.Start.Index
                    : (int)mapping.End.Index + 1;
                var text = string.Join(", ", fields.Select(pair => $"{QuoteScalar(pair.Key)}: {FormatValue(pair.Value)}"));
                edits.Add((position, position, text + (mapping.Children.Count > 0 ? ", " : string.Empty)));
            }
            else
            {
                var first = mapping.Children.First().Key;
                var indent = new string(' ', (int)first.Start.Column - 1);
                var text = string.Join(newline + indent,
                    fields.Select(pair => $"{QuoteScalar(pair.Key)}: {FormatValue(pair.Value)}"));
                edits.Add(((int)first.Start.Index, (int)first.Start.Index, text + newline + indent));
            }
        }

        var builder = new StringBuilder(original);
        var nextStart = original.Length;
        foreach (var edit in edits.OrderByDescending(edit => edit.Start))
        {
            if (edit.End > nextStart)
            {
                throw new InvalidDataException("配置包含重叠的 YAML 别名，无法安全修改。");
            }
            builder.Remove(edit.Start, edit.End - edit.Start).Insert(edit.Start, edit.Text);
            nextStart = edit.Start;
        }
        return builder.ToString();
    }

    private static string FormatValue(object value) => value is Dictionary<string, object> fields
        ? "{ " + string.Join(", ", fields.Select(pair => $"{QuoteScalar(pair.Key)}: {FormatValue(pair.Value)}")) + " }"
        : QuoteScalar((string)value);

    private static string QuoteScalar(string value) => "\"" + value
        .Replace("\\", "\\\\", StringComparison.Ordinal)
        .Replace("\"", "\\\"", StringComparison.Ordinal)
        .Replace("\r", "\\r", StringComparison.Ordinal)
        .Replace("\n", "\\n", StringComparison.Ordinal)
        .Replace("\t", "\\t", StringComparison.Ordinal)
        .Replace("\0", "\\0", StringComparison.Ordinal) + "\"";

    /// <summary>收集设置界面管理的配置路径和值。</summary>
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

    /// <summary>释放写入锁。</summary>
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
