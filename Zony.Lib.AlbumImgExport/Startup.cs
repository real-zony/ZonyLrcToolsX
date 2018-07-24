using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zony.Lib.Plugin;
using Zony.Lib.Plugin.Attributes;
using Zony.Lib.Plugin.Common;
using Zony.Lib.Plugin.Common.Extensions;
using Zony.Lib.Plugin.Interfaces;
using Zony.Lib.Plugin.Models;

namespace Zony.Lib.AlbumImgExport
{
    [PluginInfo("批量专辑图像导出", "Zony", "1.1.0.0", "http://www.myzony.com", "导出所有专辑图像到指定位置.")]
    public class Startup : IPluginExtensions, IPlugin
    {
        public Dictionary<string, Dictionary<string, object>> PluginOptions { get; set; }

        public void InitializePlugin(IPluginManager plugManager)
        {
            GlobalContext.Instance.UIContext.AddPluginButton("批量专辑图像导出", async (sender, args) =>
            {
                FolderBrowserDialog dialog = new FolderBrowserDialog();
                dialog.Description = "请选择导出的文件夹";
                dialog.ShowDialog();
                if (!Directory.Exists(dialog.SelectedPath))
                {
                    MessageBox.Show(caption: "提示", text: "请选择有效目录!", icon: MessageBoxIcon.Information, buttons: MessageBoxButtons.OK);
                    return;
                }

                string dirPath = dialog.SelectedPath;

                IEnumerable<MusicInfoModel> exports = GlobalContext.Instance.MusicInfos.Where(z => z.IsAlbumImg);

                var exportList = exports.ToList();
                GlobalContext.Instance.UIContext.Bottom_ProgressBar.Maximum = exportList.Count;
                GlobalContext.Instance.UIContext.DisableTopButtons();
                GlobalContext.Instance.SetBottomStatusText($"正在导出专辑图像");

                await Task.Run(() =>
                {
                    foreach (var item in exportList)
                    {
                        var plugin = plugManager.GetPlugin<IPluginAcquireMusicInfo>();
                        string srcName = Path.GetFileNameWithoutExtension(item.FilePath);
                        string newName = $"{srcName}.png";
                        Image.FromStream(plugin.LoadAlbumImage(item.FilePath)).Save(Path.Combine(dirPath, newName));
                        GlobalContext.Instance.SetItemStatus(item.Index, "导出成功");
                        GlobalContext.Instance.UIContext.Bottom_ProgressBar.Value++;
                    }
                });

                GlobalContext.Instance.UIContext.EnableTopButtons();
                GlobalContext.Instance.SetBottomStatusText($"专辑图像导出成功，请到 {dirPath} 查看");
            });
        }
    }
}
