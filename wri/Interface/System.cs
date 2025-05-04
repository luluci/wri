using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace wri.Interface
{
    using UtilWinApi = global::Utility.WindowsApi;

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class System
    {

        public System()
        {

        }

        public void GC()
        {
            global::System.GC.Collect();
        }
    }
}
