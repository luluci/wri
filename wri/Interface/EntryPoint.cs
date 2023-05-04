using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows;

namespace wri.Interface
{
    /// <summary>
    /// C# -> WebView2 Interfaceのルートとする。
    /// このクラス内にAPIを登録する。
    /// </summary>
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class EntryPoint
    {
        // Test用変数
        public int TestVar1 = 11;
        public int TestVar2 { get; set; } = 22;

        public EntryPoint()
        {

        }

        public void MessageBox(string msg)
        {
            System.Windows.MessageBox.Show(msg);
        }
    }
}
