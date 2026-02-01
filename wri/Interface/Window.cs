using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace wri.Interface
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class Window
    {
        public Window()
        {
        }

        public int Width
        {
            get { return (int)GlobalData.vm.Width.Value; }
            set { GlobalData.vm.Width.Value = (int)value; }
        }
        public int Height
        {
            get { return (int)GlobalData.vm.Height.Value; }
            set { GlobalData.vm.Height.Value = (int)value; }
        }

        public void SetSize(int width, int height)
        {
            GlobalData.vm.Width.Value = width;
            GlobalData.vm.Height.Value = height;
        }

        public void ShowHeader(bool show)
        {
            GlobalData.SetHeaderVisibility(show);
        }
        public void ShowFooter(bool show)
        {
            GlobalData.SetFooterVisibility(show);
        }
    }
}
