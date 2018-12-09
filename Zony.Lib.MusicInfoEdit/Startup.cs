using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Zony.Lib.Plugin;
using Zony.Lib.Plugin.Attributes;
using Zony.Lib.Plugin.Common;
using Zony.Lib.Plugin.Interfaces;

namespace Zony.Lib.MusicInfoEdit
{
    [PluginInfo("歌曲信息编辑扩展插件","Zony","1.0.0.0","https://blog.myzony.com","关于歌曲信息编辑的一些扩展功能。")]
    public class Startup : IPluginExtensions,IPlugin
    {
        public void InitializePlugin(IPluginManager plugManager)
        {
            GlobalContext.Instance.UIContext.Right_PictureBox_AlbumImage.ContextMenu = new ContextMenu(new[]
            {
                new MenuItem("保存专辑图像",
                    ((sender, args) =>
                    {
                        if (GlobalContext.Instance.UIContext.Right_PictureBox_AlbumImage.Image != null)
                        {
                            var dlg = new SaveFileDialog();
                            dlg.Title = "保存专辑图像";
                            dlg.Filter = "*.png|*.png|*.bmp|*.bmp|*.jpg|*.jpg";
                            if (dlg.ShowDialog() == DialogResult.OK)
                            {
                                using (var newFile = File.Create(dlg.FileName))
                                {
                                    GlobalContext.Instance.UIContext.Right_PictureBox_AlbumImage.Image.Save(newFile, ConvertFormat(Path.GetExtension(dlg.FileName)));
                                    newFile.Flush();
                                }
                            }
                        }
                    })),
                new MenuItem("替换专辑图像",
                    ((sender, args) =>
                    {
                        if (GlobalContext.Instance.UIContext.Center_ListViewNF_MusicList.SelectedItems.Count == 1)
                        {
                            var dlg = new OpenFileDialog();
                            dlg.Title = "准备替换的专辑图像";
                            dlg.Filter = "*.png|*.png|*.bmp|*.bmp|*.jpg|*.jpg";
                            if (dlg.ShowDialog() == DialogResult.OK)
                            {
                                using (var imageFileStream = File.Open(dlg.FileName, FileMode.Open))
                                {
                                    using (var ms = new MemoryStream())
                                    {
                                        byte[] buffer = new byte[1024 * 16];
                                        int readCount;
                                        while ((readCount = imageFileStream.Read(buffer, 0, buffer.Length)) > 0)
                                        {
                                            ms.Write(buffer,0,readCount);
                                        }

                                        var infoPlug = plugManager.GetPlugin<IPluginAcquireMusicInfo>();
                                        if (!infoPlug.SaveAlbumImage(GlobalContext.Instance.MusicInfos[GlobalContext.Instance.UIContext.Center_ListViewNF_MusicList.SelectedItems[0].Index].FilePath, ms.ToArray()))
                                        {
                                            MessageBox.Show("替换专辑图像失败.", "失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        }
                                    }
                                }
                            }
                        }
                    }))
            });
        }

        public Dictionary<string, Dictionary<string, object>> PluginOptions { get; set; }

        private ImageFormat ConvertFormat(string extensionName)
        {
            switch (extensionName)
            {
                case ".png":
                    return ImageFormat.Png;
                case ".bmp":
                    return ImageFormat.Bmp;
                case ".jpg":
                    return ImageFormat.Jpeg;
                default:
                    return ImageFormat.Png;
            }
        }
    }
}
