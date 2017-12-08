using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
