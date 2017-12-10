using Zony.Lib.Infrastructures.Dependency;

namespace ZonyLrcTools.Common.Interfaces
{
    public interface IEncodingLyricProvider : ISingletonDependency
    {
        byte[] EncodeText(string sourceStr);
        byte[] EncodeText(byte[] sourceStr);
    }
}
