using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPTools
{
    internal class RemoveEdge:Tool
    {
        public override void exe(List<string> args)
        {
            foreach (string arg in args)
            {
                string path = arg;
                string extension = Path.GetExtension(path);
                string dirpath = Path.GetDirectoryName(path);
                string filename = Path.GetFileNameWithoutExtension(path);
                string savepath = dirpath + "\\" + filename + "_cut" + extension;

                Bitmap bmp = new Bitmap(path);

                if (check(bmp))
                {

                    Color c = bmp.GetPixel(0, 0);
                    int mycolor1 = c.B;
                    int mycolor2 = c.G;
                    int mycolor3 = c.R;
                    RemoveWhiteEdge(bmp, mycolor1, mycolor2, mycolor3, savepath);
                }

            }

        }
        /// <summary>
        /// 裁剪图片（去掉百边）
        /// </summary>
        /// <param name="FilePath">图片路径</param>
        private void RemoveWhiteEdge(Bitmap bmp, int c1, int c2, int c3, string savepath)
        {
            //上左右下
            int top = 0, left = 0, right = bmp.Width, bottom = bmp.Height;

            //寻找最上面的标线,从左(0)到右，从上(0)到下
            for (int i = 0; i < bmp.Height; i++)//行
            {
                bool find = false;
                for (int j = 0; j < bmp.Width; j++)//列
                {
                    Color c = bmp.GetPixel(j, i);
                    if (!IsWhite(c, c1, c2, c3))
                    {
                        top = i;
                        find = true;
                        break;
                    }
                }
                if (find)
                    break;
            }
            //寻找最左边的标线，从上（top位）到下，从左到右
            for (int i = 0; i < bmp.Width; i++)//列
            {
                bool find = false;
                for (int j = top; j < bmp.Height; j++)//行
                {
                    Color c = bmp.GetPixel(i, j);
                    if (!IsWhite(c, c1, c2, c3))
                    {
                        left = i;
                        find = true;
                        break;
                    }
                }
                if (find)
                    break;
            }
            //寻找最下边标线，从下到上，从左到右
            for (int i = bmp.Height - 1; i >= 0; i--)//行
            {
                bool find = false;
                for (int j = left; j < bmp.Width; j++)//列
                {
                    Color c = bmp.GetPixel(j, i);
                    if (!IsWhite(c, c1, c2, c3))
                    {
                        bottom = i;
                        find = true;
                        break;
                    }
                }
                if (find)
                    break;
            }
            //寻找最右边的标线，从上到下，从右往左
            for (int i = bmp.Width - 1; i >= 0; i--)//列
            {
                bool find = false;
                for (int j = 0; j <= bottom; j++)//行
                {
                    Color c = bmp.GetPixel(i, j);
                    if (!IsWhite(c, c1, c2, c3))
                    {
                        right = i;
                        find = true;
                        break;
                    }
                }
                if (find)
                    break;
            }

            Bitmap destBitmap = new Bitmap(right - left + 1, bottom - top + 1);//目标图 
            Rectangle destRect = new Rectangle(0, 0, destBitmap.Width, destBitmap.Height);//矩形容器 
            Rectangle srcRect = new Rectangle(left, top, destBitmap.Width, destBitmap.Height);

            Graphics g = Graphics.FromImage(destBitmap);
            g.DrawImage(bmp, destRect, srcRect, GraphicsUnit.Pixel);
            destBitmap.Save(savepath);
        }
        /// <summary>
        /// 判断四周颜色是否统一
        /// </summary>
        /// <param name="bmp">输入的带边框的图像</param>
        /// <returns></returns>
        private bool check(Bitmap bmp)//
        {
            Color c = bmp.GetPixel(0, 0);
            for (int i = 0; i < bmp.Height; i++)
            {
                if (bmp.GetPixel(0, i) != c)
                    return false;
            }
            for (int i = 0; i < bmp.Height; i++)
            {
                if (bmp.GetPixel(bmp.Width - 1, i) != c)
                    return false;
            }
            for (int j = 0; j < bmp.Width; j++)
            {
                if (bmp.GetPixel(j, 0) != c)
                    return false;
            }
            for (int j = 0; j < bmp.Width; j++)
            {
                if (bmp.GetPixel(j, bmp.Height - 1) != c)
                    return false;
            }
            return true;
        }
        /// <summary>
        /// 判断是否白色和纯透明色（10点的容差）
        /// </summary>
        private bool IsWhite(Color c, int c1, int c2, int c3)
        {
            //纯透明也是白色,RGB都为255为纯白
            if ((c.R == c3 && c.G == c2 && c.B == c1))
                return true;

            return false;
        }
    }
}
