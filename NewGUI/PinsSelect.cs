using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;




namespace NewGUI
{
    public partial class PinsSelect : Form
    {
        const int WM_NCLBUTTONDOWN = 0xA1;
        const int HTCAPTION = 0x2;

        [DllImport("user32.dll")] static extern bool ReleaseCapture();
        [DllImport("user32.dll")] static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        public PinsSelect() : this("Sériový výběr") { }

        public PinsSelect(string title)
        {
            InitializeComponent();     // ← DŮLEŽITÉ

            this.MouseDown += BeginDrag;         // táhni kdekoli na pozadí Formu
            pictureBox1.MouseDown += BeginDrag;  // a i přes obrázek s deskou
            Text = title;

            // nech klidně bezborder/bez taskbaru, to nevadí
            // StartPosition dáváme MANUAL, ale pozici nastaví PositionNextToHostSafe
            StartPosition = FormStartPosition.Manual;

            KeyPreview = true;
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    e.Handled = true;
                    this.Hide();
                }
            };
        }

        private void BeginDrag(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }


        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void Pinfortytwo_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void PinTwo_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void Pin37_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void Pin1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void Pin38_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void Pin39_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void pin40_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
