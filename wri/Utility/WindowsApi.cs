using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace wri.Utility
{
    static public class WindowsApi
    {
        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_MONITORPOWER = 0xF170;
        public const int MONITOR_OFF = 2;

        [DllImport("user32.dll")]
        public static extern int SendMessage(int hWnd, int hMsg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern bool LockWorkStation();


        public static void DisplayOff()
        {
            SendMessage(-1, WM_SYSCOMMAND, SC_MONITORPOWER, MONITOR_OFF);
        }
    }
}
