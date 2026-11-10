using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;

namespace ZonyLrcTools.Desktop.Infrastructure.Configuration;

/// <summary>将含控制字符的文本输出为单行双引号标量。</summary>
public sealed class QuotedScalarEventEmitter : ChainedEventEmitter
{
    /// <summary>创建标量输出器。</summary>
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

        // 保留首尾空格，避免普通标量读取时丢失。
        return text.Length > 0 && (text[0] == ' ' || text[^1] == ' ');
    }
}
