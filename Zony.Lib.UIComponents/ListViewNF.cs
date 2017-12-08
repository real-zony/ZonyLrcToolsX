using System.Windows.Forms;

namespace Zony.Lib.UIComponents
{
    public class ListViewNF : ListView
    {
        public ListViewNF()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.EnableNotifyMessage, true);
        }

        protected override void OnNotifyMessage(Message m)
        {
            if (m.Msg != 0x14) base.OnNotifyMessage(m);
        }
    }
}
