using System;
using System.Windows.Forms;
using Zony.Lib.Infrastructures.Dependency;

namespace ZonyLrcTools.Forms
{
    public partial class Form_PluginManager : Form, ITransientDependency
    {
        public Form_PluginManager()
        {
            InitializeComponent();
        }

        private void From_PluginManager_Load(object sender, EventArgs e)
        {
            listView1.Controls.Add(new TextBox());
        }
    }
}
