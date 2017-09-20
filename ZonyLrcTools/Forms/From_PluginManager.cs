using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZonyLrcTools.Forms
{
    public partial class From_PluginManager : Form
    {
        public From_PluginManager()
        {
            InitializeComponent();
        }

        private void From_PluginManager_Load(object sender, EventArgs e)
        {
            listView1.Controls.Add(new TextBox());
        }
    }
}
