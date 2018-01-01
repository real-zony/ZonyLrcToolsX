using System.Windows.Forms;
using Zony.Lib.Infrastructures.Dependency;
using ZonyLrcTools.Common.Interfaces;

namespace ZonyLrcTools.Forms
{
    public partial class Form_Setting : Form, ITransientDependency
    {
        public IConfigurationManager ConfigurationManager { get; set; }

        public Form_Setting()
        {
            InitializeComponent();
            InitializeSettingUI();
        }

        private void InitializeSettingUI()
        {
            textBox_DownloadThreadNum.Text = ConfigurationManager.ConfigModel.DownloadThreadNumber.ToString();
        }
    }
}
