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
    
    public partial class WebpForm : Form
    {
        private string format = "jpg";
        public WebpForm()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Webp_Load(object sender, EventArgs e)
        {

        }

        

        private void button1_Click(object sender, EventArgs e)
        {
            button1.BackColor = Color.Brown;
            button2.BackColor = Color.FloralWhite;
            button3.BackColor = Color.FloralWhite;
            button4.BackColor = Color.FloralWhite;
            button5.BackColor = Color.FloralWhite;
            format = "jpg";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button1.BackColor = Color.FloralWhite;
            button2.BackColor = Color.Brown;
            button3.BackColor = Color.FloralWhite;
            button4.BackColor = Color.FloralWhite;
            button5.BackColor = Color.FloralWhite;
            format = "jpeg";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            button1.BackColor = Color.FloralWhite;
            button2.BackColor = Color.FloralWhite;
            button3.BackColor = Color.Brown;
            button4.BackColor = Color.FloralWhite;
            button5.BackColor = Color.FloralWhite;
            format = "png";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            button1.BackColor = Color.FloralWhite;
            button2.BackColor = Color.FloralWhite;
            button3.BackColor = Color.FloralWhite;
            button4.BackColor = Color.Brown;
            button5.BackColor = Color.FloralWhite;
            format = "bmp";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            button1.BackColor = Color.FloralWhite;
            button2.BackColor = Color.FloralWhite;
            button3.BackColor = Color.FloralWhite;
            button4.BackColor = Color.FloralWhite;
            button5.BackColor = Color.Brown;
            format = "gif";
        }

        public string GetFormat()
        {
            return format;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
