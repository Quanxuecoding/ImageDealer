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
    internal class ImageStitchor:Tool
    {
        public override void exe(List<string> args)
        {
            ImageStitch();
        }
        
        private void ImageStitch()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.Title = "请选择你要拼接的图片";
            dialog.Filter = "图像文件(*.jpg;*.jpeg;*.gif;*.png)|*.jpg;*.jpeg;*.gif;*.png";
            string[] imageFiles;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                imageFiles = dialog.FileNames;   
            }
            else
            {
                MessageBox.Show("图像选取出错");
                return;
            }
            if(imageFiles.Length <= 1)
            {
                MessageBox.Show("图像选取出错");
                return;
            }

            string folderName = Path.GetDirectoryName(imageFiles[0]);
            string result_name = "";
            for(int i = 0; i < imageFiles.Length; i++)
            {
                result_name += Path.GetFileNameWithoutExtension(imageFiles[i]);
                result_name += "_";
            }

            result_name += "stitch.jpg";
            
            Stitcher.Mode mode = Stitcher.Mode.Panorama;
            Mat[] imgs = new Mat[imageFiles.Length];

            //读入图像
            for (int i = 0; i < imageFiles.Length; i++)
            {
                imgs[i] = new Mat(imageFiles[i], ImreadModes.Color);
            }
            Mat pano = new Mat();
            Stitcher stitcher = Stitcher.Create(mode);
            Stitcher.Status status = stitcher.Stitch(imgs, pano);
            if (status != Stitcher.Status.OK)
            {
                Console.WriteLine("Can't stitch images, error code = {0} ", (int)status);
                return;


            }
            Cv2.ImWrite(folderName + "\\" + result_name, pano);

        }
    }
}
