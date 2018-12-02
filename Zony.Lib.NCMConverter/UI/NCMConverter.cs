using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zony.Lib.NCMConverter.Convert;

namespace Zony.Lib.NCMConverter.UI
{
    public partial class NCMConverter : Form
    {
        public Dictionary<string, Dictionary<string, object>> Params { get; set; }

        private ConcurrentDictionary<string, int> _filePaths = new ConcurrentDictionary<string, int>();

        public NCMConverter()
        {
            InitializeComponent();
        }

        private void button_selectFolder_Click(object sender, EventArgs e)
        {
            var folderDlg = new FolderBrowserDialog();
            folderDlg.Description = "请选择 NCM 文件所在的文件夹.";
            if (folderDlg.ShowDialog() == DialogResult.OK)
            {
                if (!Directory.Exists(folderDlg.SelectedPath))
                {
                    MessageBox.Show("请选择有效的文件夹路径！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                var files = NCMExtenstion.FindNCMFiles(folderDlg.SelectedPath);

                for (int i = 0; i < files.Count; i++)
                {
                    listView1.Items.Add(new ListViewItem(new[] { files[i] , ""}));
                    _filePaths.TryAdd(files[i], i);
                }
            }
        }

        private async void button_start_Click(object sender, EventArgs e)
        {
            if (_filePaths.Count == 0)
            {
                MessageBox.Show("请重新选择 NCM 文件所在的文件夹路径！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            await Task.Run(() =>
            {
                var parallelLoopResult = Parallel.ForEach(_filePaths, file =>
                {
                    var converter = new Convert.NCMConverter();
                    try
                    {
                        var result = converter.ProcessFile(file.Key);

                        if (result == NCMConverterEnum.Success)
                        {
                            listView1.Items[file.Value].SubItems[1].Text = "成功";
                        }

                        if (result == NCMConverterEnum.Invalid)
                        {
                            listView1.Items[file.Value].SubItems[1].Text = "无效文件";
                        }
                    }
                    catch (Exception)
                    {
                        listView1.Items[file.Value].SubItems[1].Text = "失败";
                    }
                });

                if (parallelLoopResult.IsCompleted)
                {
                    _filePaths.Clear();
                }
            });
        }
    }
}