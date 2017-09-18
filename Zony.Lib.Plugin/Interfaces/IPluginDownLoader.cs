namespace Zony.Lib.Plugin.Interfaces
{
    /// <summary>
    /// 下载器插件
    /// </summary>
    public interface IPluginDownLoader
    {
        void DownLoad(string songName, string artistName, out byte[] data);
    }
}