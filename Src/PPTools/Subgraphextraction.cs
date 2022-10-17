using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PPTools
{
    internal class Subgraphextraction:Tool
    {
        
        private static ArrayList CornerPointX;
        private static ArrayList CornerPointY;
        private static string extension;
        private SubExtractorForm progress;
        private int images;
        private int tasks;
        private int step;
        private int value;
        private Thread processBarThread;

        /// <summary>
        /// 对args中指定的路径的图片分别进行子图提取
        /// </summary>
        /// <param name="args"></param>
        public override void exe(List<string> args)
        {
            progress = new SubExtractorForm();
            images = args.Count;
            tasks = 8;
            step = 100 / (images * tasks);
            value = 0;
            foreach(string arg in args)
            {
                CornerPointX = new ArrayList();
                CornerPointY = new ArrayList();

                string path = arg;

                SubExtract(path);
                progress.SetProgerssBar(100);
                processBarThread.Abort();
            }
        }


        /// <summary>
        /// 对path路径的图片进行子图提取
        /// </summary>
        /// <param name="path"></param>
        public void SubExtract(string path)
        {
            string filename = Path.GetFileNameWithoutExtension(path);
            extension = Path.GetExtension(path);
            string dirpath = Path.GetDirectoryName(path);
            string outfile = dirpath + "\\" + filename;
            processBarThread = new Thread(new ThreadStart(ShowProcess));
            processBarThread.Start();
            //对图像进行一系列处理
            Bitmap rgbImage = new Bitmap(path);
            Bitmap grayImage = Rgb2Gray(rgbImage);

            Bitmap Image = CopyEdge(grayImage);
            int[,] h1 = new int[3, 3]
            {
                { -1, -1, -1},
                {  2,  2,  2},
                { -1, -1, -1}
            };
            int[,] h2 = new int[3, 3]
            {
                { -1, 2, -1},
                { -1, 2, -1},
                { -1, 2, -1}
            };
            Addprogess();
            Bitmap Image1 = imfilter(Image, h1);
            Addprogess();
            Bitmap Image2 = imfilter(Image, h2);
            Addprogess();



            Bitmap DivedImage = new Bitmap(Image1.Width, Image1.Height);
            for (int i = 0; i < Image1.Width; i++)
            {
                for (int j = 0; j < Image1.Height; j++)
                {
                    Color color1 = Image1.GetPixel(i, j);
                    Color color2 = Image2.GetPixel(i, j);
                    int R = color1.R + color2.R;
                    int G = color1.G + color2.G;
                    int B = color1.B + color2.B;
                    R = R > 0 ? R : 0;
                    G = G > 0 ? G : 0;
                    B = B > 0 ? B : 0;
                    R = R < 256 ? R : 255;
                    G = G < 256 ? G : 255;
                    B = B < 256 ? B : 255;
                    Color color = Color.FromArgb(R, G, B);
                    DivedImage.SetPixel(i, j, color);
                }
            }
            Addprogess();
            Bitmap ProcessedImage = RemoveEdge(DivedImage);
            Addprogess();
            Bitmap MaskImage = GenMask(ProcessedImage);
            Addprogess();
            GetCornerPoints(MaskImage);
            Addprogess();
            SaveChildImages(rgbImage, outfile);
            Addprogess();

        }

        private void ShowProcess()
        {
            Application.Run(progress);
        }

        private void Addprogess()
        {

            value += step;
            progress.SetProgerssBar(value);
        }


        /// <summary>
        /// 剪切图像的指定部分
        /// </summary>
        /// <param name="url">图像地址</param>
        /// <param name="beginX">开始的x坐标</param>
        /// <param name="beginY">开始的y坐标</param>
        /// <param name="getX">x方向的长度</param>
        /// <param name="getY">y方向的长度</param>
        /// <param name="fileName">要保存的图片名称</param>
        /// <param name="savePath">要保存的路径</param>
        /// <param name="fileExt">要保存的图片的格式</param>
        /// <returns></returns>
        private static string CutImage(string url, int beginX, int beginY, int getX, int getY, string fileName, string savePath, string fileExt)
        {
            if ((beginX < getX) && (beginY < getY))
            {
                Bitmap bitmap = new Bitmap(url);//原图 
                if (((beginX + getX) <= bitmap.Width) && ((beginY + getY) <= bitmap.Height))
                {
                    Bitmap destBitmap = new Bitmap(getX, getY);//目标图 
                    Rectangle destRect = new Rectangle(0, 0, getX, getY);//矩形容器 
                    Rectangle srcRect = new Rectangle(beginX, beginY, getX, getY);

                    Graphics g = Graphics.FromImage(destBitmap);
                    g.DrawImage(bitmap, destRect, srcRect, GraphicsUnit.Pixel);


                    destBitmap.Save(savePath + "//" + fileName);
                    return savePath + "\\" + "*" + fileName.Split('.')[0] + "." + fileExt;
                }
                else
                {
                    return "截取范围超出图片范围";
                }
            }
            else
            {
                return "请确认(beginX < getX)&&(beginY < getY)";
            }
        }

        /// <summary>
        /// 将rgb图片转换成灰度图
        /// </summary>
        /// <param name="rgbImage">rgb图片</param>
        /// <returns>rgb色图片对应的灰度图</returns>
        private static Bitmap Rgb2Gray(Bitmap rgbImage)
        {
            Bitmap bmp = new Bitmap(rgbImage.Width, rgbImage.Height);
            Color curColor;
            int ret;
            int Width = rgbImage.Width;
            int Height = rgbImage.Height;
            //循环读取像素转换灰度值
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    curColor = rgbImage.GetPixel(i, j);
                    ret = (int)(curColor.R * 0.299 + curColor.G * 0.587 + curColor.B * 0.114);
                    bmp.SetPixel(i, j, Color.FromArgb(ret, ret, ret));
                }
            }


            return bmp;
        }

        /// <summary>
        /// 判断图片是否是一个图片
        /// </summary>
        /// <param name="path">图片的路径</param>
        /// <returns></returns>
        private static bool IsRealImage(string path)
        {
            try
            {
                Image img = Image.FromFile(path);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// 拓宽边界，方便卷积核运算
        /// </summary>
        /// <param name="inputImage">输入的图片</param>
        /// <returns></returns>
        private static Bitmap CopyEdge(Bitmap inputImage)
        {
            int width = inputImage.Width;
            int height = inputImage.Height;
            //创建边界宽以像素的图片
            Bitmap outputBitmap = new Bitmap(width + 2, height + 2);  //拓宽边缘
            Rectangle destRect = new Rectangle(1, 1, width, height);//矩形容器 
            Rectangle srcRect = new Rectangle(0, 0, width, height);
            //将原图片复制到宽的图片的中间
            Graphics g = Graphics.FromImage(outputBitmap);
            g.DrawImage(inputImage, destRect, srcRect, GraphicsUnit.Pixel);

            g.Dispose();

            try
            {
                Color color;
                //横向填充边框
                for (int i = 0; i < outputBitmap.Width; i++)
                {
                    color = outputBitmap.GetPixel(i, 1);
                    outputBitmap.SetPixel(i, 0, color);

                    color = outputBitmap.GetPixel(i, outputBitmap.Height - 2);
                    outputBitmap.SetPixel(i, outputBitmap.Height - 1, color);

                }
                //纵向填充边框
                for (int i = 0; i < outputBitmap.Height; i++)
                {
                    color = outputBitmap.GetPixel(1, i);
                    outputBitmap.SetPixel(0, i, color);

                    color = outputBitmap.GetPixel(outputBitmap.Width - 2, i);
                    outputBitmap.SetPixel(outputBitmap.Width - 1, i, color);
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show("像素操作超出图片尺寸索引");
            }




            return outputBitmap;
        }

        /// <summary>
        /// 对图像进行卷积处理
        /// </summary>
        /// <param name="bitmap">输入的图像 </param>
        /// <param name="h">卷积后的图像</param>
        /// <returns></returns>
        private static Bitmap imfilter(Bitmap bitmap, int[,] h)
        {

            int[,,] InputPicture = new int[3, bitmap.Width, bitmap.Height];//以GRB以及位图的长宽建立整数输入的位图的数组

            Color color;//储存某一像素的颜色
            //循环使得InputPicture数组得到位图的RGB
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    color = bitmap.GetPixel(i, j);
                    InputPicture[0, i, j] = color.R;
                    InputPicture[1, i, j] = color.G;
                    InputPicture[2, i, j] = color.B;
                }
            }


            Bitmap outputBitmap = new Bitmap(bitmap.Width, bitmap.Height);//创建新位图
            //循环计算使得OutputPicture数组得到计算后位图的RGB
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    int R = 0;
                    int G = 0;
                    int B = 0;
                    //每一个像素计算使用输入的卷积核进行计算
                    for (int r = 0; r < h.GetLength(0); r++)//循环卷积核的每一行
                    {
                        for (int f = 0; f < h.GetLength(1); f++)//循环卷积核的每一列
                        {
                            //控制与卷积核相乘的元素
                            int row = i - 1 + r;
                            int col = j - 1 + f;

                            //当超出位图的大小范围时，选择最边缘的像素值作为该点的像素值
                            row = row < 0 ? 0 : row;
                            col = col < 0 ? 0 : col;
                            row = row >= bitmap.Width ? bitmap.Width - 1 : row;
                            col = col >= bitmap.Height ? bitmap.Height - 1 : col;

                            //输出得到像素的RGB值
                            R += h[r, f] * InputPicture[0, row, col];
                            G += h[r, f] * InputPicture[1, row, col];
                            B += h[r, f] * InputPicture[2, row, col];


                        }
                    }
                    R = R > 0 ? R : 0;
                    G = G > 0 ? G : 0;
                    B = B > 0 ? B : 0;
                    R = R < 256 ? R : 255;
                    G = G < 256 ? G : 255;
                    B = B < 256 ? B : 255;
                    color = Color.FromArgb(R, G, B);//颜色结构储存该点RGB
                    outputBitmap.SetPixel(i, j, color);//位图存储该点像素值
                }
            }


            return outputBitmap;
        }

        /// <summary>
        /// 去除边框
        /// </summary>
        /// <param name="inputImage">输入图片</param>
        /// <returns></returns>
        private static Bitmap RemoveEdge(Bitmap inputImage)
        {
            Bitmap outputImage = new Bitmap(inputImage.Width - 2, inputImage.Height - 2);
            Rectangle destRect = new Rectangle(0, 0, outputImage.Width, outputImage.Height);//矩形容器 
            Rectangle srcRect = new Rectangle(1, 1, outputImage.Width, outputImage.Height);
            Graphics g = Graphics.FromImage(outputImage);
            g.DrawImage(inputImage, destRect, srcRect, GraphicsUnit.Pixel);
            return outputImage;
        }

        /// <summary>
        /// 根据分好边界的图像生成子图蒙版
        /// </summary>
        /// <param name="inputImage">输入图像</param>
        /// <returns></returns>
        private static Bitmap GenMask(Bitmap inputImage)
        {


            ArrayList xcount = new ArrayList();
            ArrayList ycount = new ArrayList();

            for (int i = 0; i < inputImage.Width; i++)
            {
                int sum = 0;
                for (int j = 0; j < inputImage.Height; j++)
                {
                    sum += inputImage.GetPixel(i, j).R;//之前已经将rgb转化为灰度图所以取r、g、b中的一个就行了
                }
                if (sum == 0)
                {
                    xcount.Add(i);
                }
            }

            for (int j = 0; j < inputImage.Height; j++)
            {
                int sum = 0;
                for (int i = 0; i < inputImage.Width; i++)
                {
                    sum += inputImage.GetPixel(i, j).R;//之前已经将rgb转化为灰度图所以取r、g、b中的一个就行了
                }
                if (sum == 0)
                {
                    ycount.Add(j);
                }
            }

            Bitmap outputImage = new Bitmap(inputImage.Width, inputImage.Height);
            //初始化为全白
            for (int i = 0; i < inputImage.Width; i++)
            {
                for (int j = 0; j < inputImage.Height; j++)
                {
                    Color color = Color.FromArgb(255, 255, 255);
                    outputImage.SetPixel(i, j, color);
                }
            }
            //根据前面统计的边界行在蒙版上分割子图
            foreach (int i in xcount)
            {
                for (int j = 0; j < inputImage.Height; j++)
                {
                    Color color = Color.FromArgb(0, 0, 0);
                    outputImage.SetPixel(i, j, color);
                }
            }

            foreach (int j in ycount)
            {
                for (int i = 0; i < inputImage.Width; i++)
                {
                    Color color = Color.FromArgb(0, 0, 0);
                    outputImage.SetPixel(i, j, color);
                }
            }

            return outputImage;
        }

        /// <summary>
        /// 根据分割好的蒙版识别出角点
        /// </summary>
        /// <param name="inputImage">输入的图片</param>
        private static void GetCornerPoints(Bitmap inputImage)
        {
            int R;
            R = inputImage.GetPixel(0, 0).R;
            if (R == 255)
            {
                CornerPointX.Add(0);
                CornerPointY.Add(0);
            }
            R = inputImage.GetPixel(0, inputImage.Height - 1).R;
            if (R == 255)
            {
                CornerPointX.Add(0);
                CornerPointY.Add(inputImage.Height - 1);
            }
            R = inputImage.GetPixel(inputImage.Width - 1, 0).R;
            if (R == 255)
            {
                CornerPointX.Add(inputImage.Width - 1);
                CornerPointY.Add(0);
            }
            R = inputImage.GetPixel(inputImage.Width - 1, inputImage.Height - 1).R;
            if (R == 255)
            {
                CornerPointX.Add(inputImage.Width - 1);
                CornerPointY.Add(inputImage.Height - 1);
            }

            for (int i = 1; i < inputImage.Width - 1; i++)
            {
                int cur, front, back;
                cur = inputImage.GetPixel(i, 0).R;
                front = inputImage.GetPixel(i - 1, 0).R;
                back = inputImage.GetPixel(i + 1, 0).R;
                if (cur == 255 && (front == 0 || back == 0))
                {
                    CornerPointX.Add(i);
                    CornerPointY.Add(0);
                }

                cur = inputImage.GetPixel(i, inputImage.Height - 1).R;
                front = inputImage.GetPixel(i - 1, inputImage.Height - 1).R;
                back = inputImage.GetPixel(i + 1, inputImage.Height - 1).R;
                if (cur == 255 && (front == 0 || back == 0))
                {
                    CornerPointX.Add(i);
                    CornerPointY.Add(inputImage.Height - 1);
                }

            }

            for (int j = 1; j < inputImage.Height - 1; j++)
            {
                int cur, front, back;
                cur = inputImage.GetPixel(0, j).R;
                front = inputImage.GetPixel(0, j - 1).R;
                back = inputImage.GetPixel(0, j + 1).R;
                if (cur == 255 && (front == 0 || back == 0))
                {
                    CornerPointX.Add(0);
                    CornerPointY.Add(j);
                }

                cur = inputImage.GetPixel(inputImage.Width - 1, j).R;
                front = inputImage.GetPixel(inputImage.Width - 1, j - 1).R;
                back = inputImage.GetPixel(inputImage.Width - 1, j + 1).R;
                if (cur == 255 && (front == 0 || back == 0))
                {
                    CornerPointX.Add(inputImage.Width - 1);
                    CornerPointY.Add(j);
                }
            }

            for (int i = 1; i < inputImage.Width - 1; i++)
            {
                for (int j = 1; j < inputImage.Height - 1; j++)
                {

                    int cur = 0, sum = 0;
                    cur = inputImage.GetPixel(i, j).R;
                    for (int ii = i - 1; ii <= i + 1; ii++)
                    {
                        for (int jj = j - 1; jj <= j + 1; jj++)
                        {
                            sum += inputImage.GetPixel(ii, jj).R;
                        }
                    }
                    if (cur == 255 && sum == 1020)
                    {
                        CornerPointX.Add(i);
                        CornerPointY.Add(j);
                    }

                }
            }

        }

        /// <summary>
        /// 根据识别出的角点将子图保存在指定路径
        /// </summary>
        /// <param name="inputImage">输入图片</param>
        /// <param name="path">保存路径</param>
        private static void SaveChildImages(Bitmap inputImage, string path)
        {

            ArrayList BoundaryCoordinatesX = new ArrayList();
            ArrayList BoundaryCoordinatesY = new ArrayList();
            foreach (int x in CornerPointX)
            {
                if (!BoundaryCoordinatesX.Contains(x))
                {
                    BoundaryCoordinatesX.Add(x);
                }
            }
            foreach (int y in CornerPointY)
            {
                if (!BoundaryCoordinatesY.Contains(y))
                {
                    BoundaryCoordinatesY.Add(y);
                }
            }
            BoundaryCoordinatesX.Sort();
            BoundaryCoordinatesY.Sort();

            int count = 0;
            for (int i = 0; i < BoundaryCoordinatesX.Count; i += 2)
            {
                for (int j = 0; j < BoundaryCoordinatesY.Count; j += 2)
                {

                    int x1 = (int)BoundaryCoordinatesX[i];
                    int y1 = (int)BoundaryCoordinatesY[j];
                    int x2 = (int)BoundaryCoordinatesX[i + 1];
                    int y2 = (int)BoundaryCoordinatesY[j + 1];

                    int width = x2 - x1 + 1;
                    int height = y2 - y1 + 1;

                    Bitmap bitmap = new Bitmap(width, height);
                    Rectangle destRect = new Rectangle(0, 0, width, height);//矩形容器 
                    Rectangle srcRect = new Rectangle(x1, y1, width, height);
                    Graphics g = Graphics.FromImage(bitmap);
                    g.DrawImage(inputImage, destRect, srcRect, GraphicsUnit.Pixel);
                    g.Dispose();
                    count++;
                    string savePath = path + "_" + count.ToString() + extension;


                    Bitmap pBitmap = PostProcess(bitmap);


                    pBitmap.Save(savePath);

                }
            }

        }

        private static Bitmap PostProcess(Bitmap bmp)
        {
            if (check(bmp))
            {
                Color c = bmp.GetPixel(0, 0);
                int mycolor1 = c.B;
                int mycolor2 = c.G;
                int mycolor3 = c.R;
                Bitmap outputBitmap = RemoveWhiteEdge(bmp, mycolor1, mycolor2, mycolor3);
                return outputBitmap;
            }
            return bmp;
        }

        /// <summary>
        /// 裁剪图片（去掉百边）
        /// </summary>
        /// <param name="FilePath">图片路径</param>
        private static Bitmap RemoveWhiteEdge(Bitmap bmp, int c1, int c2, int c3)
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

            return destBitmap;
        }
        /// <summary>
        /// 判断四周颜色是否统一
        /// </summary>
        /// <param name="bmp">输入的带边框的图像</param>
        /// <returns></returns>
        private static bool check(Bitmap bmp)//
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
        private static bool IsWhite(Color c, int c1, int c2, int c3)
        {
            //纯透明也是白色,RGB都为255为纯白
            if ((c.R == c3 && c.G == c2 && c.B == c1))
                return true;

            return false;
        }
    }

}
