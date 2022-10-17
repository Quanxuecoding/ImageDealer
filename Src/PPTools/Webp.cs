using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Aspose.Imaging;
using Aspose.Imaging.ImageOptions;

namespace PPTools
{
    internal class Webp:Tool
    {
        public override void exe(List<string> args)
        {
            foreach(string arg in args)
            {
                string path = arg;
                TurnOfWebp(path);
            }
        }
        private void TurnOfWebp(string path)
        {
            string extension = Path.GetExtension(path);
            string dirPath = Path.GetDirectoryName(path);
            string fileName = Path.GetFileNameWithoutExtension(path);
            if (extension == ".webp")
            {
                WebpForm form = new WebpForm();
                Application.Run(form);
                string tar = form.GetFormat();
                switch (tar)
                {
                    case "jpg":
                        using (Image image = Image.Load(path))
                        {
                            image.Save(dirPath + '\\' + fileName + '.' + tar, new JpegOptions());
                        }
                        break;
                    case "jpeg":
                        using (Image image = Image.Load(path))
                        {
                            image.Save(dirPath + '\\' + fileName + '.' + tar, new JpegOptions());
                        }
                        break;
                    case "png":
                        using (Image image = Image.Load(path))
                        {
                            image.Save(dirPath + '\\' + fileName + '.' + tar, new PngOptions());
                        }
                        break;
                    case "bmp":
                        using (Image image = Image.Load(path))
                        {
                            image.Save(dirPath + '\\' + fileName + '.' + tar, new BmpOptions());
                        }
                        break;
                    case "gif":
                        using (Image image = Image.Load(path))
                        {
                            image.Save(dirPath + '\\' + fileName + '.' + tar, new GifOptions());
                        }
                        break;
                    default: break;
                }
            }
            else
            {
                using (Image image = Image.Load(path))
                {
                    image.Save(dirPath + '\\' + fileName + ".webp", new WebPOptions());
                }

            }
        }
    }
}
