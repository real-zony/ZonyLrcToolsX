using Zony.Lib.Infrastructures.Dependency;

namespace ZonyLrcTools.Common.Interfaces
{
    public interface IDownLoader : ITransientDependency
    {
        byte[] DownLoad();
    }
}
