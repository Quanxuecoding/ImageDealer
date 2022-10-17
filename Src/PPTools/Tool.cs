using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PPTools
{
    public class Tool
    {
        public virtual void exe(List<string> args)
        {
            MessageBox.Show("没有对应功能，未进行任何操作");
        }
    }
}
