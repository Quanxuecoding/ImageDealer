using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPTools
{
    internal class InverseColor:Tool
    {
        public override void exe(List<string> args)
        {
            foreach(string arg in args)
            {
                string path = arg;
                Inverse(path);
            }
        }
        public void Inverse(string path)
        {
            string dirPath = Path.GetDirectoryName(path);
            string name = Path.GetFileNameWithoutExtension(path);
            string ext = Path.GetExtension(path);

            Mat img = Cv2.ImRead(path);
            for (int i = 0; i < img.Rows; i++)
            {
                for (int j = 0; j < img.Cols; j++)
                {
                    img.At<int>(i, j) = 255 - img.At<int>(i, j);
                }
            }

            Cv2.ImWrite(dirPath + '\\' + name + "_反色" + ext, img);
        }
    }
}
