using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace PPTools
{
    class PPTools
    {
       
        [STAThread]  //声明为单线程
        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                MessageBox.Show("没有处理的对象");
                return;
            }

            PPTools tool = new PPTools();
            string command = (string)args.GetValue(args.Length - 1);
            List<string>paths =  args.Take(args.Length - 1).ToList();
            tool.execCommand(command, paths);

            return;
        }

        /// <summary>
        /// 根据命令选择合适的功能执行
        /// </summary>
        /// <param name="command">处理图片要使用的功能</param>
        /// <param name="arg">需要处理的对象</param>

        private void execCommand(string command, List<string> args)
        {
            List<string> argPaths = new List<string>();
            foreach (string path in args)
            {
                string argPath = string.Format("\"{0}\"", path);
                argPaths.Add(argPath);
            }
            string cmdArg = string.Join(" ", argPaths);
            string rootPath = getRootPath();
            rootPath = rootPath + @"\Tools";
            string appFile;

            Tool tool;

            switch(command)
            {
                case "getImageData":

                    tool = new getImageData();
                    tool.exe(args);
                    //appFile = string.Format(@"{0}\{1}", rootPath, "图像数据读取\\图像数据读取.exe");
                    //Process.Start(appFile, cmdArg);
                    break;
                case "Subgraphextraction":
                    tool = new Subgraphextraction();
                    tool.exe(args);
                    
                    break;
                case "RemoveEdge":
                    tool = new RemoveEdge();
                    tool.exe(args);
                    
                    break;
                case "LosslessAmplification":
                    tool = new LosslessAmplification();
                    tool.exe(args);
                    
                    break;
                case "HandwritingExtraction":
                    tool = new HandwritingExtraction();
                    tool.exe(args);
                    
                    break;
                case "BrightnessStretching":
                    tool = new BrightnessStretching();
                    tool.exe(args);
                    
                    break;
                case "CurveExtraction":
                    appFile = string.Format(@"{0}\{1}", rootPath, "CurveExtraction\\main.exe");
                    Process exep = new Process();
                    exep.StartInfo.FileName = appFile;
                    exep.StartInfo.UseShellExecute = false;
                    exep.StartInfo.CreateNoWindow = true;
                    exep.Start();
                    exep.WaitForExit();
                    
                    break;
                case "webp":
                    tool = new Webp();
                    tool.exe(args);
                    break;
                case "InverseColor":
                    tool = new InverseColor();
                    tool.exe(args);
                    break;
                case "imageStitch":
                    tool = new ImageStitchor();
                    tool.exe(args);
                    break;
                case "TiltCorrect":
                    appFile = string.Format(@"{0}\{1}", rootPath, "TiltCorrection\\TiltCorrect.exe");
                    Process.Start(appFile, cmdArg);
                    break;
                case "ImageVectorize":
                    tool = new ImageVectorize();
                    tool.exe(args);
                    break;
                default:
                    tool = new Tool();
                    tool.exe(args);
                    break;
            }  

        }

        private string getRootPath()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);   //通过 CodeBase 得到一个 URI 格式的路径；
            string path = Uri.UnescapeDataString(uri.Path); //用 UriBuild.UnescapeDataString 去掉前缀 File://；
            return Path.GetDirectoryName(path);  //用 GetDirectoryName 把它变成正常的 windows 格式。
        }

    }

}
