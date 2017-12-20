using System.Windows.Forms;

namespace Zony.Lib.UIComponents.Box
{
    public partial class InputBox : Form
    {
        public string ResultText { get; set; }

        public InputBox()
        {
            InitializeComponent();
        }

        public InputBox(string title, string text) : this()
        {
            this.Text = title;
            label1.Text = text;
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            ResultText = textBox1.Text;
            Close();
        }

        private void button2_Click(object sender, System.EventArgs e)
        {
            Close();
        }
    }
}
