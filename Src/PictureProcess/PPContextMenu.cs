using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using SharpShell.SharpContextMenu;
using SharpShell.Attributes;


namespace PictureProcess
{
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.AllFiles), COMServerAssociation(AssociationType.Directory)]  //folder包括控制面板等其他的，这里directory就足够了
    public class PPContextMenu:SharpContextMenu
    {
        protected override bool CanShowMenu()
        {
            return true;
        }

        protected override ContextMenuStrip CreateMenu()
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            ToolStripMenuItem item = new ToolStripMenuItem("PictureProcess");
            //item.Image = Properties.Resources.pp;
            //item.ImageScaling = ToolStripItemImageScaling.None;
            //item.ImageTransparentColor = System.Drawing.Color.White;
            //item.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            Dictionary<string, string> subItemInfo = new Dictionary<string, string>();
            subItemInfo.Add("提取图片数据", "getImageData");
            subItemInfo.Add("拼接图片", "imageStitch");
            subItemInfo.Add("子图提取", "Subgraphextraction");
            subItemInfo.Add("边框处理", "RemoveEdge");
            subItemInfo.Add("无损放大", "LosslessAmplification");
            subItemInfo.Add("笔迹提取", "HandwritingExtraction");
            subItemInfo.Add("自动亮度拉伸", "BrightnessStretching");
            subItemInfo.Add("曲线提取", "CurveExtraction");
            subItemInfo.Add("webp格式图片转换", "webp");
            subItemInfo.Add("图片反色", "InverseColor");
            subItemInfo.Add("倾斜矫正", "TiltCorrect");
            subItemInfo.Add("图片矢量化", "ImageVectorize");
            foreach (KeyValuePair<string, string> kv in subItemInfo)
            {
                ToolStripMenuItem subItem = new ToolStripMenuItem(kv.Key);
                subItem.Click += (o, e) =>
                {
                    Item_Click(o, e, kv.Value);
                };
                item.DropDownItems.Add(subItem);
            }

            menu.Items.Add(item);
            return menu;
        }
        private void Item_Click(object sender, EventArgs e, string command)
        {
            string rootPath = getRootPath();
            string appFile = string.Format(@"{0}\{1}", rootPath, "PPTools.exe");
            if (!File.Exists(appFile))
            {
                MessageBox.Show(string.Format("找不到程序路径{0}{1}", Environment.NewLine, appFile), "出错了", MessageBoxButtons.OK);
                //Environment.NewLine获取当前环境中的回车换行字符串。
                return;
            }
            List<string> paths = SelectedItemPaths.ToList();
            //将路径加上双引号避免路径中空格引起的参数格式错误
            List<string> argPaths = new List<string>();
            foreach(string path in paths)
            {
                string argPath = string.Format("\"{0}\"", path);
                argPaths.Add(argPath);
            }

            argPaths.Add(command);

            string args = string.Join(" ", argPaths);
            //MessageBox.Show(args);
            Process.Start(appFile, args);
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
