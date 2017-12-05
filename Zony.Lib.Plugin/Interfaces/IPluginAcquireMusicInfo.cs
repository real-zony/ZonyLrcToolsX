using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Zony.Lib.Plugin.Models;

namespace Zony.Lib.Plugin.Interfaces
{
    public interface IPluginAcquireMusicInfo
    {
        /// <summary>
        /// 获得歌曲的详细信息
        /// </summary>
        /// <param name="musicFiles">歌曲路径列表</param>
        /// <returns>歌曲信息列表</returns>
        List<MusicInfoModel> GetMusicInfos(Dictionary<string, List<string>> musicFiles);
        /// <summary>
        /// 获得歌曲的详细信息
        /// </summary>
        /// <param name="musicFiles">歌曲路径列表</param>
        /// <returns>歌曲信息列表</returns>
        Task<List<MusicInfoModel>> GetMusicInfosAsync(Dictionary<string, List<string>> musicFiles);
        /// <summary>
        /// 获得歌曲的详细信息
        /// </summary>
        /// <param name="filePath">歌曲路径</param>
        /// <returns>歌曲信息</returns>
        MusicInfoModel GetMusicInfo(string filePath);
        /// <summary>
        /// 加载歌曲专辑图像
        /// </summary>
        /// <param name="filePath">歌曲路径</param>
        /// <returns>图像流</returns>
        Stream LoadAlbumImage(string filePath);
        /// <summary>
        /// 保存歌曲专辑图像
        /// </summary>
        /// <param name="filePath">歌曲路径</param>
        /// <param name="imageStream">图像流</param>
        /// <returns>是否保存成功，TRUE 成功，FALSE 失败</returns>
        bool SaveAlbumImage(string filePath, Stream imageStream);
        /// <summary>
        /// 保存歌曲专辑图像
        /// </summary>
        /// <param name="filePath">歌曲路径</param>
        /// <param name="imgBytes">图像数据</param>
        /// <returns>是否保存成功，TRUE 成功，FALSE 失败</returns>
        bool SaveAlbumImage(string filePath, byte[] imgBytes);
        /// <summary>
        /// 保存歌曲信息
        /// </summary>
        /// <param name="filePath">歌曲路径</param>
        /// <param name="musicInfo">新的歌曲信息</param>
        /// <param name="E">保存失败的时候抛出的异常信息</param>
        /// <returns>是否保存成功，TRUE 成功，FALSE 失败</returns>
        bool SaveMusicInfo(string filePath, MusicInfoModel musicInfo, out Exception E);
    }
}
