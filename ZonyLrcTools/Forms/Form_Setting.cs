using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zony.Lib.Infrastructures.Common.Interfaces;
using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Infrastructures.EventBus;
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
                textBox_proxyIP.Text = ConfigurationManager.ConfigModel.ProxyIP;
                textBox_porxyPort.Text = ConfigurationManager.ConfigModel.ProxyPort.ToString();

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
            ConfigurationManager.ConfigModel.ProxyIP = textBox_proxyIP.Text;
            ConfigurationManager.ConfigModel.ProxyPort = int.TryParse(textBox_porxyPort.Text, out int proxyPort) ? proxyPort : 0;
        }
    }
}