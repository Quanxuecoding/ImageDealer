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
    public partial class BrightnessStretchingForm : Form
    {
        public BrightnessStretchingForm()
        {
            InitializeComponent();
        }

        private void BrightnessStretchingForm_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            double value = (double)trackBar1.Value / 100;
            label5.Text = value.ToString();
        }
        public double getBarValue()
        {
            return (double)trackBar1.Value / 100;
        }
        public void setBrightnessValue(double value)
        {
            label7.Text = value.ToString();
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }
    }
}
