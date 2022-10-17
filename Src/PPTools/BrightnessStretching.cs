using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PPTools
{
    internal class BrightnessStretching:Tool
    {
        public override void exe(List<string> args)
        {
            foreach (string arg in args)
            {
                string path = arg;


                Mat img = new Mat();
                img = Cv2.ImRead(path);

                BrightnessStretch(img, path);
            }
        }
        public static void BrightnessStretch(Mat img, string path)
        {
            Mat streted_img = new Mat();


            Mat grayimg = new Mat();
            Cv2.CvtColor(img, grayimg, ColorConversionCodes.BGR2GRAY);
            Scalar scalar = Cv2.Mean(grayimg);
            double imglight = scalar.Val0;

            double brightness = imglight / 255;

            double minimum_brightness;

            BrightnessStretchingForm slideBar = new BrightnessStretchingForm();
            slideBar.setBrightnessValue(brightness);
            Application.Run(slideBar);
            minimum_brightness = Convert.ToDouble(slideBar.getBarValue());


            double ratio = brightness / minimum_brightness;
            Cv2.ConvertScaleAbs(img, streted_img, 1 / ratio, 0);

            string extension = Path.GetExtension(path);
            string filename = Path.GetFileNameWithoutExtension(path);
            string filepath = Path.GetDirectoryName(path);

            if (ratio < 1)
                Cv2.ImWrite(filepath + "\\" + filename + "_加亮" + extension, streted_img);
            else
                Cv2.ImWrite(filepath + "\\" + filename + "_调暗" + extension, streted_img);
        }
    }
}
