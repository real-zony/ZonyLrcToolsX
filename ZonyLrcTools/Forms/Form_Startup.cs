using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Zony.Lib.Infrastructures.Dependency;
using ZonyLrcTools.Common;

namespace ZonyLrcTools.Forms
{
    public partial class Form_Startup : Form, ITransientDependency
    {
        private readonly ISearchProvider m_searchProvider;

        public Form_Startup(ISearchProvider searchProvider)
        {
            InitializeComponent();

            m_searchProvider = searchProvider;
        }

        private void Form_Startup_Load(object sender, EventArgs e)
        {
        }

        private void button_SearchFile_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog _dlg = new FolderBrowserDialog()
            {
                Description = "请选择歌曲所在目录.",
            };

            if (_dlg.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(_dlg.SelectedPath))
            {
                List<string> _files = m_searchProvider.FindFiles(_dlg.SelectedPath);

            }
        }

        private void button_DownloadLyric_Click(object sender, EventArgs e)
        {

        }
    }
}
