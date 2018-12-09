using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Infrastructures.EventBus;
using ZonyLrcTools.Common.Interfaces;
using ZonyLrcTools.Events;

namespace ZonyLrcTools.Forms
{
    public partial class Form_Setting : Form, ITransientDependency
    {
        public IConfigurationManager ConfigurationManager { get; set; }

        public Form_Setting()
        {
            InitializeComponent();
        }

        private async void Form_Setting_Load(object sender, System.EventArgs e)
        {
            await InitializeSettingUI();
        }

        private void button_SaveAndExit_Click(object sender, System.EventArgs e)
        {
            SerializeUI();
            EventBus.Default.Trigger<ProgramExitEventData>();
            Close();
        }

        /// <summary>
        /// 根据 ConfigrationModel 数据初始化 UI
        /// </summary>
        private async Task InitializeSettingUI()
        {
            await Task.Run(() =>
            {
                // 加载编码页
                Encoding.GetEncodings().ToList().ForEach(encoding => comboBox_EncodingPages.Items.Add(encoding.Name.ToUpper()));
                comboBox_EncodingPages.Items.Add("UTF-8 BOM");
                comboBox_EncodingPages.Items.Add("ANSI");
                comboBox_EncodingPages.Items.Add("UTF-16");
                comboBox_EncodingPages.Items.Add("GBK");
                comboBox_EncodingPages.Text = ConfigurationManager.ConfigModel.EncodingName;

                textBox_ExtensionsName.Text = string.Join(";", ConfigurationManager.ConfigModel.ExtensionsName.ToArray());
                textBox_DownloadThreadNum.Text = ConfigurationManager.ConfigModel.DownloadThreadNumber.ToString();
                textBox_PluginOptions.Text = JsonConvert.SerializeObject(ConfigurationManager.ConfigModel.PluginOptions);
                checkBox_IsReplaceLyricFile.Checked = ConfigurationManager.ConfigModel.IsReplaceLyricFile;
                checkBox_IsCheckUpdate.Checked = ConfigurationManager.ConfigModel.IsCheckUpdate;
            });
        }

        /// <summary>
        /// 序列化 UI 信息到 ConfigurationModel
        /// </summary>
        private void SerializeUI()
        {
            ConfigurationManager.ConfigModel.DownloadThreadNumber = int.Parse(textBox_DownloadThreadNum.Text);
            ConfigurationManager.ConfigModel.EncodingName = comboBox_EncodingPages.Text;
            ConfigurationManager.ConfigModel.ExtensionsName = textBox_ExtensionsName.Text.Split(';').ToList();
            ConfigurationManager.ConfigModel.IsReplaceLyricFile = checkBox_IsReplaceLyricFile.Checked;
            ConfigurationManager.ConfigModel.IsCheckUpdate = checkBox_IsCheckUpdate.Checked;
            ConfigurationManager.ConfigModel.PluginOptions = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(textBox_PluginOptions.Text);
        }

        private async void button_selectProxiesFile_Click(object sender,
            System.EventArgs e)
        {
            if (MessageBox.Show($"请选择有效的代理文件，每个代理条目以: \r\n" +
                                " <代理服务器 IP>,<代理服务器端口>,<用户名>,<密码> 形式构成。\r\n" +
                                $"每个代理条目以换行为分隔符，如果有多个代理条目请注意格式。",
                    "提示", MessageBoxButtons.OK, MessageBoxIcon.Information) == DialogResult.OK)
            {
                var fileDlg = new OpenFileDialog();
                fileDlg.Filter = "*.txt|*.txt";
                fileDlg.Title = "请选择存放有代理服务器列表的 TXT 文件";
                if (fileDlg.ShowDialog() == DialogResult.OK)
                {
                    if (!File.Exists(fileDlg.FileName)) MessageBox.Show("无效的文件路径.", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    var proxies = string.Empty;
                    using (var proxyFile = File.Open(fileDlg.FileName, FileMode.Open))
                    {
                        using (var reader = new StreamReader(proxyFile))
                        {
                            proxies = await reader.ReadToEndAsync();
                        }
                    }


                }
            }
        }
    }
}