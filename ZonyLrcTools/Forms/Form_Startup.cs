using System;
using System.Windows.Forms;
using ZonyLrcTools.EventsMethod;

namespace ZonyLrcTools.Forms
{
    public partial class Form_Startup : Form
    {
        public Form_Startup()
        {
            InitializeComponent();
        }

        private void Form_Startup_Load(object sender, EventArgs e)
        {
        }

        private void ui_EventBind()
        {
            button_SearchFile.Click += delegate { SearchFileEvent.EventCallMethod(); };
        }
    }
}
