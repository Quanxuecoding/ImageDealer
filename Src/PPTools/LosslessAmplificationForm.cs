using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PPTools
{
    public partial class LosslessAmplificationForm : Form
    {
        private char way = 'n';
        private int scale = 2;
        public LosslessAmplificationForm()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            scale = Convert.ToInt32(textBox1.Text);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            button3.BackColor = Color.Brown;
            button4.BackColor = Color.FloralWhite;
            way = 'y';
        }

        private void button4_Click(object sender, EventArgs e)
        {
            button3.BackColor = Color.FloralWhite;
            button4.BackColor = Color.Brown;
            way = 'n';
        }
        public char getWay()
        {
            return way;
        }
        public int getScale()
        {
            return scale;
        }
    }
}
