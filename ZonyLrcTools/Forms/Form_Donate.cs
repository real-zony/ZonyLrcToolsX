using System;
using System.Windows.Forms;
using Zony.Lib.Infrastructures.Dependency;

namespace ZonyLrcTools.Forms
{
    public partial class Form_Donate : Form,ITransientDependency
    {
        public Form_Donate()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
