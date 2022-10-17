using OpenCvSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PPTools
{
    internal class HandwritingExtraction:Tool
    {
        public override void exe(List<string> args)
        {
            foreach (string arg in args)
            {
                string path = arg;
                HandwritingExtract(arg);
            }
        }
        public static void HandwritingExtract(string fullName)
        {
            string extension = Path.GetExtension(fullName);
            string fileName = Path.GetFileNameWithoutExtension(fullName);
            string dirPath = Path.GetDirectoryName(fullName);
            //if (!Directory.Exists(dirPath)) 可能可以用来替代exist
            Mat originalImg = Cv2.ImRead(fullName);

            Mat resized_img = new Mat();

            Size img_size = originalImg.Size();
            int width = img_size.Width;
            int height = img_size.Height;

            double ratio = 0.0;
            const double width_limit = 800.0;
            const double height_limit = 600.0;


            Mat imgROI = new Mat();


            if (width > width_limit || height > height_limit)
            {
                double w_ratio = width_limit / width;
                double h_ratio = height_limit / height;

                ratio = w_ratio > h_ratio ? h_ratio : w_ratio;

                Cv2.Resize(originalImg, resized_img, new Size(), ratio, ratio, InterpolationFlags.Area);
                Rect roi = Cv2.SelectROI("图片裁剪", resized_img, false);
                //Cv2.SelectROIs

                roi.X = (int)(roi.X / ratio);
                roi.Y = (int)(roi.Y / ratio);
                roi.Width = (int)(roi.Width / ratio);
                roi.Height = (int)(roi.Height / ratio);

                if (roi.Width != 0 && roi.Height != 0)
                    imgROI = originalImg[roi];
                else
                    imgROI = originalImg;

            }
            else
            {
                resized_img = originalImg.Clone();

                Rect roi = Cv2.SelectROI("图片裁剪", resized_img, false);

                if (roi.Width != 0 && roi.Height != 0)
                    imgROI = originalImg[roi];
                else
                    imgROI = originalImg;
            }


            //Cv2.ImShow("resized_原始图片", resized_img);
            //Cv2.WaitKey();

            //Cv2.ImShow("裁剪图片", imgROI);
            //Cv2.WaitKey();
            Mat grayImg = new Mat();

            Cv2.CvtColor(imgROI, grayImg, ColorConversionCodes.BGR2GRAY);

            //Cv2.ImShow("灰度图片", grayImg);
            //Cv2.WaitKey();


            //底帽滤波
            int borderWidth = 4;
            Size s = new Size(19, 19);
            Scalar scalar = 1;
            Mat ele = new Mat(s, MatType.CV_8U, scalar);
            for (int i = 0; i < borderWidth; i++)
            {
                for (int j = 0; j < borderWidth; j++)
                {
                    if (i + j < borderWidth)//int可能不太对
                    {
                        ele.At<byte>(i, j) = 0;
                        ele.At<byte>(i, ele.Cols - 1 - j) = 0;
                        ele.At<byte>(ele.Rows - 1 - i, j) = 0;
                        ele.At<byte>(ele.Rows - 1 - i, ele.Cols - 1 - j) = 0;
                    }
                }
            }
            Mat g = new Mat();
            Cv2.MorphologyEx(grayImg, g, MorphTypes.BlackHat, ele);

            //二值化
            Mat BW = new Mat();
            Cv2.Threshold(g, BW, 0, 255, ThresholdTypes.Otsu);

            //平滑笔划边界
            Size s2 = new Size(3, 3);
            Mat SE1 = Cv2.GetStructuringElement(MorphShapes.Cross, s2);
            Cv2.MorphologyEx(BW, BW, MorphTypes.Open, SE1);
            Size s3 = new Size(3, 3);
            Mat SE2 = Cv2.GetStructuringElement(MorphShapes.Rect, s3);
            Cv2.MorphologyEx(BW, BW, MorphTypes.Open, SE2);


            //以BW为模板，提取原始灰度图像
            Size img_size_crop = imgROI.Size();
            Mat result_pic = imgROI;

            if (imgROI.Channels() == 3)
            {
                for (int i = 0; i < BW.Rows; i++)
                {
                    for (int j = 0; j < BW.Cols; j++)
                    {
                        if (BW.At<byte>(i, j) == 0)
                        {
                            result_pic.At<Vec3b>(i, j)[0] = 255;
                            result_pic.At<Vec3b>(i, j)[1] = 255;
                            result_pic.At<Vec3b>(i, j)[2] = 255;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < BW.Rows; i++)
                {
                    for (int j = 0; j < BW.Cols; j++)
                    {
                        if (BW.Get<byte>(i, j) == 0)
                        {
                            result_pic.Set<byte>(i, j, 255);
                        }
                    }
                }
            }

            //Cv2.NamedWindow("白背景", WindowFlags.Normal);
            //Cv2.ResizeWindow("白背景", result_pic.Size());
            //Cv2.ImShow("白背景", result_pic);
            //Cv2.WaitKey();

            //灰度 / RGB通道，数值范围自动拉伸到 0~255

            //先分情况找到最大最小值
            int MaxI = 0;

            if (result_pic.Channels() == 3)
            {
                for (int i = 0; i < result_pic.Rows; i++)
                {
                    for (int j = 0; j < result_pic.Cols; j++)
                    {
                        MaxI = result_pic.At<Vec3b>(i, j)[0] > MaxI ? result_pic.At<Vec3b>(i, j)[0] : MaxI;
                    }
                }
            }
            else
            {
                for (int i = 0; i < result_pic.Rows; i++)
                {
                    for (int j = 0; j < result_pic.Cols; j++)
                    {
                        MaxI = result_pic.At<byte>(i, j) > MaxI ? result_pic.At<byte>(i, j) : MaxI;
                    }
                }
            }

            int MinI = 256;

            if (result_pic.Channels() == 3)
            {
                for (int i = 0; i < result_pic.Rows; i++)
                {
                    for (int j = 0; j < result_pic.Cols; j++)
                    {
                        MinI = result_pic.At<Vec3b>(i, j)[0] < MinI ? result_pic.At<Vec3b>(i, j)[0] : MinI;
                    }
                }
            }
            else
            {
                for (int i = 0; i < result_pic.Rows; i++)
                {
                    for (int j = 0; j < result_pic.Cols; j++)
                    {
                        MinI = result_pic.At<byte>(i, j) < MinI ? result_pic.At<byte>(i, j) : MinI;
                    }
                }
            }


            //都减去最小值/最大值*255就完成了拉伸，正好最小值是零，最大值达到255
            if (result_pic.Channels() == 3)
            {
                for (int i = 0; i < result_pic.Rows; i++)
                {
                    for (int j = 0; j < result_pic.Cols; j++)
                    {
                        result_pic.At<Vec3b>(i, j)[0] = (byte)((result_pic.At<Vec3b>(i, j)[0] - MinI) / (double)MaxI * 255);
                        result_pic.At<Vec3b>(i, j)[1] = (byte)((result_pic.At<Vec3b>(i, j)[1] - MinI) / (double)MaxI * 255);
                        result_pic.At<Vec3b>(i, j)[2] = (byte)((result_pic.At<Vec3b>(i, j)[2] - MinI) / (double)MaxI * 255);
                    }
                }
            }
            else
            {
                for (int i = 0; i < result_pic.Rows; i++)
                {
                    for (int j = 0; j < result_pic.Cols; j++)
                    {
                        result_pic.At<byte>(i, j) = (byte)((result_pic.At<byte>(i, j) - MinI) / (double)MaxI * 255);
                    }
                }
            }


            //Cv2.ImShow("拉伸", result_pic);
            //Cv2.WaitKey();


            //确定文字区域

            int[] row_Sum = new int[BW.Rows];
            int[] col_Sum = new int[BW.Cols];


            //利用二值模板为量像素总和来确定该行是否有笔记划过
            for (int i = 0; i < BW.Rows; i++)
            {
                row_Sum[i] = 0;
                for (int j = 0; j < BW.Cols; j++)
                {
                    if (BW.At<byte>(i, j) == 0)
                    {
                        row_Sum[i] = row_Sum[i] + 1;
                    }

                }
            }

            for (int j = 0; j < BW.Cols; j++)
            {
                col_Sum[j] = 0;
                for (int i = 0; i < BW.Rows; i++)
                {
                    if (BW.At<byte>(i, j) == 0)
                    {
                        col_Sum[j] = col_Sum[j] + 1;
                    }

                }
            }

            ArrayList X = new ArrayList();
            ArrayList Y = new ArrayList();

            //如果和不和最白你上一致则有笔记划过所以记录
            for (int i = 1; i < row_Sum.GetLength(0); i++)
            {
                if (row_Sum[i] != row_Sum[0])
                {
                    X.Add(i);
                }
            }
            for (int i = 1; i < col_Sum.GetLength(0); i++)
            {
                if (col_Sum[i] != col_Sum[0])
                {
                    Y.Add(i);
                }
            }
            X.Sort();
            Y.Sort();
            //得到的xy的最大最小值就可以确定一个方框边界，那就是笔记区域。

            Rect crop = new Rect(Convert.ToInt32(Y[0]), Convert.ToInt32(X[0]), Convert.ToInt32(Y[Y.Count - 1]) - Convert.ToInt32(Y[0]) + 1, Convert.ToInt32(X[X.Count - 1]) - Convert.ToInt32(X[0]) + 1);
            //注意这里rect的x和width对应的是mat的cols，y和height对应的是mat的rows

            Mat res_cropped = new Mat(result_pic, crop);
            Mat BW_cropped = new Mat(BW, crop);

            Size size_cropped = res_cropped.Size();


            //Cv2.ImShow("确定区域", res_cropped);
            //Cv2.WaitKey();

            // 添加白色边框

            const int margin_Width = 20;

            //构造扩充边框的size
            Size size_res = new Size();
            size_res.Width = size_cropped.Width + 2 * margin_Width;
            size_res.Height = size_cropped.Height + 2 * margin_Width;

            //扩充原图

            Mat result_pic2 = new Mat(size_res, MatType.CV_8U, 255);

            if (res_cropped.Channels() == 3)
            {
                for (int i = 0; i < res_cropped.Rows; i++)
                {
                    for (int j = 0; j < res_cropped.Cols; j++)
                    {
                        result_pic2.At<Vec3b>(i + 20, j + 20)[0] = res_cropped.At<Vec3b>(i, j)[0];
                        result_pic2.At<Vec3b>(i + 20, j + 20)[1] = res_cropped.At<Vec3b>(i, j)[1];
                        result_pic2.At<Vec3b>(i + 20, j + 20)[2] = res_cropped.At<Vec3b>(i, j)[2];
                    }
                }
            }
            else
            {
                for (int i = 0; i < res_cropped.Rows; i++)
                {
                    for (int j = 0; j < res_cropped.Cols; j++)
                    {
                        result_pic2.At<byte>(i + 20, j + 20) = res_cropped.At<byte>(i, j);
                    }
                }
            }


            //扩充二值模板

            Mat Result_BW = new Mat(size_res, MatType.CV_8U, 0);

            if (BW_cropped.Channels() == 3)
            {
                for (int i = 0; i < BW_cropped.Rows; i++)
                {
                    for (int j = 0; j < BW_cropped.Cols; j++)
                    {
                        Result_BW.At<Vec3b>(i + 20, j + 20)[0] = BW_cropped.At<Vec3b>(i, j)[0];
                        Result_BW.At<Vec3b>(i + 20, j + 20)[1] = BW_cropped.At<Vec3b>(i, j)[1];
                        Result_BW.At<Vec3b>(i + 20, j + 20)[2] = BW_cropped.At<Vec3b>(i, j)[2];
                    }
                }
            }
            else
            {
                for (int i = 0; i < res_cropped.Rows; i++)
                {
                    for (int j = 0; j < res_cropped.Cols; j++)
                    {
                        Result_BW.At<byte>(i + 20, j + 20) = BW_cropped.At<byte>(i, j);
                    }
                }
            }

            //Cv2.ImShow("扩充边框1", result_pic2);
            //Cv2.WaitKey();

            //Cv2.ImShow("扩充边框2", Result_BW);
            //Cv2.WaitKey();


            //保存结果到透明背景的png格式文件

            Mat transparent_background_img = new Mat();
            //添加alpha透明度通道
            if (result_pic2.Channels() == 3)
            {
                Cv2.CvtColor(result_pic2, transparent_background_img, ColorConversionCodes.RGB2RGBA);
            }
            else
            {
                Cv2.CvtColor(result_pic2, transparent_background_img, ColorConversionCodes.GRAY2RGBA);
            }

            //根据bw对应的扩展模板把对应的背景转换成透明的
            for (int i = 0; i < Result_BW.Rows; i++)
            {
                for (int j = 0; j < Result_BW.Cols; j++)
                {
                    if (Result_BW.At<byte>(i, j) == 0)
                    {
                        transparent_background_img.At<Vec4b>(i, j)[3] = 0;
                    }
                    else
                    {
                        transparent_background_img.At<Vec4b>(i, j)[3] = 255;
                    }
                }
            }
            //必须是png格式才可以体现出透明度
            Cv2.ImWrite(dirPath + "\\" + fileName + "_signature" + ".png", transparent_background_img);

        }
    }
}
