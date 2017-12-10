using Zony.Lib.Infrastructures.Dependency;

namespace ZonyLrcTools.Common.Interfaces
{
    public interface IEncodingLyricProvider : ISingletonDependency
    {
        string EncodeText(string sourceStr);
        string EncodeText(byte[] sourceStr);
    }
}
