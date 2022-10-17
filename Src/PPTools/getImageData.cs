using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;


namespace PPTools
{
    internal class getImageData:Tool
    {
        public override void exe(List<string> args)
        {
            string text = "";
            //每一个arg都是一个图片文件的地址
            foreach (string arg in args)
            {
                try
                {
                    Bitmap bmp = new Bitmap(arg);
                    //Console.WriteLine(arg+"'s information: ");
                    //Console.WriteLine(bmp.Width);
                    //Console.WriteLine(bmp.Height);
                    //Console.WriteLine(bmp.PixelFormat);
                    //拼接剪贴板的字符串
                    text = text + arg + "'s information: " +
                        " Width " + bmp.Width +
                        ", Height " + bmp.Height +
                        ", PixelFrmat " + bmp.PixelFormat + Environment.NewLine;

                }
                catch (Exception e)
                {
                    MessageBox.Show(arg + " is not an image");
                }
            }         
            try
            {
                Clipboard.SetDataObject(text, true);
                MessageBox.Show("图片信息已经存入剪贴板");
            }
            catch
            {
                MessageBox.Show("写入剪贴板失败！");
            }
            return;
        }

    }
}
