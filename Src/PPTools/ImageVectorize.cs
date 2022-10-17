using Aspose.Imaging.ImageOptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aspose.Imaging;


namespace PPTools
{
    internal class ImageVectorize:Tool
    {
        public override void exe(List<string> args)
        {
            foreach(string arg in args)
            {
                string path = arg;
                Vectorize(path);
            }
        }
        private void Vectorize(string path)
        {
            string dirPath = Path.GetDirectoryName(path);
            string name = Path.GetFileNameWithoutExtension(path);
            string ext = Path.GetExtension(path);

            Image image = Image.Load(path);
            VectorRasterizationOptions vectorRasterizationOptions = new SvgRasterizationOptions() { PageSize = image.Size };
            image.Save(dirPath + '\\' + name + ".svg", new SvgOptions() { VectorRasterizationOptions = vectorRasterizationOptions });
        }
    }
}
