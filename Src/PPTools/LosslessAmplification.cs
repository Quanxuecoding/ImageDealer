using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.DnnSuperres;

namespace PPTools
{
    internal class LosslessAmplification:Tool
    {
        public override void exe(List<string> args)
        {
    
            char way = 'n';
            int scale = 2;
            LosslessAmplificationForm Form = new LosslessAmplificationForm();
            Application.Run(Form);
            way = Form.getWay();
            scale = Form.getScale();


            string curdir = AppDomain.CurrentDomain.BaseDirectory;
            TimeSpan costTime = new TimeSpan(0);
            foreach (string arg in args)
            {
                string path = arg;
                string dirpath = Path.GetDirectoryName(path);
                string filename = Path.GetFileNameWithoutExtension(path);
                string extension = Path.GetExtension(path);

                DnnSuperResImpl sr = new DnnSuperResImpl();
                Mat img = Cv2.ImRead(path);

                if (way == 'y')
                {
                    if (scale != 2 && scale != 3 && scale != 4)
                    {
                        scale = 2;
                    }
                }
                else
                {
                    if (scale != 2 && scale != 3 && scale != 4 && scale != 8)
                    {
                        scale = 2;
                    }
                }

                string modelPath = "";//这里有个小问题，modelPath只能使用绝对路径，相对路径readmodel会报错，也就是说每到一台新电脑都要改（不知道怎么解决）
                string label = "";
                if (way == 'y')
                {
                    modelPath = String.Format(curdir + "\\model\\{0}_x{1}.pb", "EDSR", scale);
                    label = "edsr";
                }
                else
                {
                    if (scale == 8)
                    {
                        modelPath = String.Format(curdir + "\\model\\{0}_x{1}.pb", "LapSRN", scale);
                        label = "lapsrn";
                    }
                    else
                    {
                        modelPath = String.Format(curdir + "\\model\\{0}_x{1}.pb", "ESPCN", scale);
                        label = "espcn";
                    }
                }
                sr.ReadModel(modelPath);

                sr.SetModel(label, scale);

                Stopwatch sw = new Stopwatch();
                sw.Start();
                Mat result = new Mat();
                sr.Upsample(img, result);
                sw.Stop();

                if (way != 'y')
                    Cv2.ImWrite(dirpath + "\\" + filename + "_rewrite_" + scale.ToString() + "x" + extension, result);
                else
                    Cv2.ImWrite(dirpath + "\\" + filename + "_highQuality_rewrite_" + scale.ToString() + "x" + extension, result);

                costTime = costTime + sw.Elapsed;

            }
        }

    }
}
