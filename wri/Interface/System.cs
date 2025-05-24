using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace wri.Interface
{
    using UtilWinApi = global::Utility.WindowsApi;
    using Console = global::Utility.Console;

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class System
    {
        public ConsoleIf Console { get; set; }

        public System()
        {
            Console = new ConsoleIf();
        }

        public void GC()
        {
            global::System.GC.Collect();
        }
    }

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class ConsoleIf
    {
        Console console;

        public ConsoleIf()
        {
            console = new Console();
        }

        public bool Start()
        {
            return console.Start(OnStdout, OnExit);
        }
        public void SetBash()
        {
            console.SetBash();
        }
        public void Close()
        {
            console.Close();
        }

        private void OnStdout(string stdout)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                var json = Json.MakeJsonStringConsoleStdout(stdout);
                GlobalData.WebView2.CoreWebView2.PostWebMessageAsJson(json);
            }));
        }
        private void StdoutPost(string stdout)
        {
            var json = Json.MakeJsonStringConsoleStdout(stdout);
            GlobalData.WebView2.CoreWebView2.PostWebMessageAsJson(json);
        }
        private void OnExit(int code)
        {
            //ExitCode = console.ExitCode;
        }

        public int ExitCode { get; set; } = 0;
        public string ErrorMessage { get; set; } = string.Empty;

        public async void ExecCmd(object param)
        {
            try
            {
                if (param is object[] ps)
                {
                    var list = ps.Select(x => x as string).ToArray();
                    await console.ExecCmdAsync(list);
                }
                else
                {
                    var str = param as string;
                    await console.ExecCmdAsync(str);
                }
                ErrorMessage = console.ErrorMessage;
                ExitCode = console.ExitCode;
                var json = Json.MakeJsonStringConsoleExit(ExitCode);
                GlobalData.WebView2.CoreWebView2.PostWebMessageAsJson(json);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                ExitCode = -1;
                var json = Json.MakeJsonStringConsoleExit(ExitCode);
                GlobalData.WebView2.CoreWebView2.PostWebMessageAsJson(json);
            }
        }
    }
}
