/* 这个 http://shimat.github.io/opencvsharp/api/OpenCvSharp.DnnSuperres.DnnSuperResImpl.html#OpenCvSharp_DnnSuperres_DnnSuperResImpl_ReadModel_System_String_
 网址有类内全部函数*/
//声明：输出图片质量和输入图片质量直接挂钩，只具有无损放大功能，不能超分辨
using OpenCvSharp;
using OpenCvSharp.DnnSuperres;
using Point = OpenCvSharp.Point;
using System;
using System.Diagnostics;

DnnSuperResImpl sr = new DnnSuperResImpl();

Console.WriteLine("请输入文件的绝对路径: ");
string path = Console.ReadLine();
Console.WriteLine("请输入文件名: ");
string name = Console.ReadLine(); 
Mat img = Cv2.ImRead(path + '\\' + name);

//加入实现获取文件名和文件格式的部分
string extension = Path.GetExtension(name);
string filename = Path.GetFileNameWithoutExtension(name);

Console.WriteLine("若想要启用更高质量(但是运行速度会明显下降)输入y, 否则输入n：");
string strway = Console.ReadLine();
char way = Convert.ToChar(strway);
Console.WriteLine("请输入目标放大倍数：(支持2/3/4/8, 默认执行2, 更高质量不支持8)");
string strscale = Console.ReadLine();
int scale = Convert.ToInt32(strscale);
if(way == 'y')
{
    if(scale != 2 && scale != 3 && scale != 4)
    {
        scale = 2;
    }
}
else
{
    if(scale != 2 && scale != 3 && scale != 4 && scale != 8)
    {
        scale = 2;
    }
}

string modelPath = "";//这里有个小问题，modelPath只能使用绝对路径，相对路径readmodel会报错，也就是说每到一台新电脑都要改（不知道怎么解决）
string label = "";
if(way == 'y')
{
    modelPath = String.Format("D:\\大二上\\大创\\图像无损放大\\Opencv_sharp\\ImageLoader\\ImageLoader\\model\\{0}_x{1}.pb", "EDSR", scale);
    label = "edsr";
}
else
{
    if(scale == 8)
    {
        modelPath = String.Format("D:\\大二上\\大创\\图像无损放大\\Opencv_sharp\\ImageLoader\\ImageLoader\\model\\{0}_x{1}.pb", "LapSRN", scale);
        label = "lapsrn";
    }
    else
    {
        modelPath = String.Format("D:\\大二上\\大创\\图像无损放大\\Opencv_sharp\\ImageLoader\\ImageLoader\\model\\{0}_x{1}.pb", "ESPCN", scale);
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

if(way != 'y')
    Cv2.ImWrite(path + "\\" + filename + "_rewrite_" + scale.ToString() + "x" + extension, result);
else
    Cv2.ImWrite(path + "\\" + filename + "_highQuality_rewrite_" + scale.ToString() + "x" + extension, result);
Console.WriteLine("任务用时: " + sw.Elapsed.ToString());
Console.WriteLine("按任意键结束...");
Console.ReadKey();
