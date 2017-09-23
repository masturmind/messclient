using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace messclient
{
    public partial class PopForm : Form
    {
        //handling any error messages and bringing back the starting Form2
        public PopForm(string info)
        {
            InitializeComponent();
            label1.Text = info;
            AcceptButton = button1;
            messclient.Form1.dead = true;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            var f = new Form2();
            f.Closed += (s, args) => this.Close();
            f.Show();

        }
    }
}
