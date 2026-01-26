using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.Web.WebView2.Wpf;

namespace wri.Interface
{
    static public class GlobalData
    {
        static public WebView2 WebView2;

        public delegate void SetVisibility(bool isVisible);
        static public SetVisibility SetHeaderVisibility = (isVisible) => {};
        static public SetVisibility SetFooterVisibility = (isVisible) => { };
    }

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

        // DragDrop操作フラグ
        public bool isDragDropInProgress { get; set; } = false;
        public string droppedFilePath { get; set; } = null;

        // Interfaceクラスインスタンス
        public SystemIf system { get; set; }
        public WindowsApi win { get; set; }
        public IO io { get; set; }
        public Config config { get; set; }
        public Window window { get; set; }

        public EntryPoint(MainWindowViewModel vm)
        {
            system = new SystemIf();
            win = new WindowsApi();
            io = new IO();
            config = new Config();
            window = new Window(vm);

            // test
            //win.GetLogOnOffEventLogList(win.MachineName);
        }

        public void Debug(object obj)
        {
            return;
        }

        public void MessageBox(string msg)
        {
            global::System.Windows.MessageBox.Show(msg);
        }
    }
}
