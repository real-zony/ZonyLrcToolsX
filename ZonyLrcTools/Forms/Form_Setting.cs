using System.Windows.Forms;
using Zony.Lib.Infrastructures.Dependency;
using ZonyLrcTools.Common.Interfaces;

namespace ZonyLrcTools.Forms
{
    public partial class Form_Setting : Form, ITransientDependency
    {
        private readonly IConfigurationManager m_settingMgr;

        public Form_Setting(IConfigurationManager settingMgr)
        {
            InitializeComponent();
            m_settingMgr = settingMgr;
        }
    }
}
