using System.Windows.Forms;
using Zony.Lib.Infrastructures.Dependency;
using ZonyLrcTools.Common;

namespace ZonyLrcTools.Forms
{
    public partial class Form_Setting : Form, ITransientDependency
    {
        private readonly ConfigurationManager m_settingMgr;

        public Form_Setting(ConfigurationManager settingMgr)
        {
            InitializeComponent();
            m_settingMgr = settingMgr;
        }
    }
}
