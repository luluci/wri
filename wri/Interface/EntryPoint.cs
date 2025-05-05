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

        // WebView2側に公開しない変数:プロパティにしなければ見えないはず
        public Uri Source;
        public string SourcePath { get; set; } = string.Empty;

        // アプリ制御フラグ
        public bool preventClose { get; set; } = false; // true:アプリ終了を防ぐ
        public string preventCloseMsg { get; set; } = string.Empty; // 終了防止メッセージ

        // Interfaceクラスインスタンス
        public System system { get; set; }
        public WindowsApi win { get; set; }
        public IO io { get; set; }

        public EntryPoint()
        {
            system = new System();
            win = new WindowsApi();
            io = new IO();

            // test
            //win.GetLogOnOffEventLogList(win.MachineName);
        }


        public void MessageBox(string msg)
        {
            global::System.Windows.MessageBox.Show(msg);
        }
    }
}
