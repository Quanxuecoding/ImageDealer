using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class tutorial60
    {
        public void Run()
        {
            //bool divide_images = false;
            Stitcher.Mode mode = Stitcher.Mode.Panorama;

            string folderName = @"D:/test/";
            /*string[] imageFiles = { "003.png", "004.png", "005.png",
                "006.png", "007.png", "008.png" };*/
            string[] imageFiles = { "007.png", "008.png", "006.png" };

            string result_name = "result.jpg";
            Mat[] imgs = new Mat[imageFiles.Length];

            //读入图像
            for (int i = 0; i < imageFiles.Length; i++)
            {
                imgs[i] = new Mat(folderName + imageFiles[i], ImreadModes.Color);
            }

            Mat pano = new Mat();
            Stitcher stitcher = Stitcher.Create(mode);
            Stitcher.Status status = stitcher.Stitch(imgs, pano);
            if (status != Stitcher.Status.OK)
            {
                /*Console.WriteLine("Can't stitch images, error code = {0} ", (int)status);
                return;*/
                if(imgs.Length == 2)
                {
                    /*Mat[] img = new Mat[2];                   
                    Mat img0 = new Mat();*/
                    rotate(imgs[0], 90, out imgs[0]);
                    using (new Window(result_name, imgs[0], WindowFlags.Normal))
                        Cv2.WaitKey();
                    rotate(imgs[1], 90, out imgs[1]);
                    using (new Window(result_name, imgs[1], WindowFlags.Normal))
                        Cv2.WaitKey();
                    stitcher.Stitch(imgs, pano);
                    using (new Window(result_name, pano, WindowFlags.Normal))
                        Cv2.WaitKey();
                    rotate(pano, -90, out pano);
                    Cv2.ImWrite(folderName + result_name, pano);
                    using (new Window(result_name, pano, WindowFlags.Normal))
                        Cv2.WaitKey();
                    return;
                }

            }
            Cv2.ImWrite(folderName + result_name, pano);
            using (new Window(result_name, pano, WindowFlags.Normal))
                Cv2.WaitKey();
        }
        public void rotate(Mat src, float angle, out Mat dst)
        {
            dst = new Mat();
            Point2f center = new Point2f(src.Cols / 2, src.Rows / 2);
            Mat rot = Cv2.GetRotationMatrix2D(center, angle, 1);
            Size2f s2f = new Size2f(src.Size().Width, src.Size().Height);
            Rect box = new RotatedRect(new Point2f(0, 0), s2f, angle).BoundingRect();
            double xx = rot.At<double>(0, 2) + box.Width / 2 - src.Cols / 2;
            double zz = rot.At<double>(1, 2) + box.Height / 2 - src.Rows / 2;
            rot.Set(0, 2, xx);
            rot.Set(1, 2, zz);
            Cv2.WarpAffine(src, dst, rot, box.Size);
        }
        //rot.At<double>(0,2)不能直接用等于号赋值，要用set赋值
        public static void Main()
        {
            tutorial60 t = new tutorial60();
            t.Run();
        }
    }
}
