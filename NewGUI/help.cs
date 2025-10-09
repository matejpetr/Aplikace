using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NewGUI
{
    public partial class help : UserControl

    {
        private Form1 _rodic;
        public help(Form1 rodic)
        {
            InitializeComponent();
            _rodic = rodic;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            _rodic.NahraditObsah(new Documentation(_rodic));
        }
    }
}
