using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;

namespace ZonyLrcTools.Desktop.Infrastructure.Configuration;

/// <summary>
/// Forces every control-character scalar onto a single double-quoted line.
/// </summary>
/// <remarks>
/// <para>
/// YamlDotNet renders a value such as <c>"\n"</c> as the folded block scalar <c>&gt;2</c>, which is
/// technically legal YAML but reads as garbage to anyone opening <c>config.yaml</c> by hand and
/// contradicts the documentation that describes <c>lineBreak</c> as an escape sequence.
/// </para>
/// <para>
/// This emitter only changes presentation: the value that survives a round trip is identical,
/// it just comes back as <c>lineBreak: "\n"</c> instead of a folded block.
/// </para>
/// </remarks>
public sealed class QuotedScalarEventEmitter : ChainedEventEmitter
{
    public QuotedScalarEventEmitter(IEventEmitter nextEmitter) : base(nextEmitter)
    {
    }

    /// <inheritdoc />
    public override void Emit(ScalarEventInfo eventInfo, IEmitter emitter)
    {
        if (eventInfo.Source.Value is string text && NeedsQuoting(text))
        {
            eventInfo.Style = ScalarStyle.DoubleQuoted;
        }

        base.Emit(eventInfo, emitter);
    }

    private static bool NeedsQuoting(string text)
    {
        foreach (var c in text)
        {
            if (c is '\n' or '\r' or '\t' or '"' or '\\')
            {
                return true;
            }
        }

        // Leading or trailing whitespace is silently trimmed by the plain-scalar reader.
        return text.Length > 0 && (text[0] == ' ' || text[^1] == ' ');
    }
}
