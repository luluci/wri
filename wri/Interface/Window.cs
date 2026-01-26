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
        public MainWindowViewModel vm;

        public Window(MainWindowViewModel viewModel)
        {
            vm = viewModel;
        }

        public void SetSize(int width, int height)
        {
            vm.Width.Value = width;
            vm.Height.Value = height;
        }
    }
}
