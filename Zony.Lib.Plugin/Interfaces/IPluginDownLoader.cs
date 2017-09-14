namespace Zony.Lib.Plugin.Interfaces
{
    public interface IPluginDownLoader
    {
        bool DownLoad(string songName, string artistName, out byte[] data);
    }
}