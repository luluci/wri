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
        public ProcessIf Process { get; set; }

        public System()
        {
            Console = new ConsoleIf();
            Process = new ProcessIf();
        }

        public void GC()
        {
            global::System.GC.Collect();
        }
    }

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class ProcessIf
    {
        public void Start(string path, string arguments = "")
        {
            //var process = new global::System.Diagnostics.Process();
            //process.StartInfo.FileName = path;
            //process.StartInfo.Arguments = arguments;
            //process.Start();

            var startInfo = new global::System.Diagnostics.ProcessStartInfo()
            {
                FileName = path,
                UseShellExecute = true,
                CreateNoWindow = true,
                Arguments = arguments,
            };
            global::System.Diagnostics.Process.Start(startInfo);
        }
        public void Kill(int pid)
        {
            try
            {
                var process = global::System.Diagnostics.Process.GetProcessById(pid);
                process.Kill();
            }
            catch (Exception ex)
            {
                // Handle exception if needed
            }
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

        public void SetExitCodeStr(string cmd)
        {
            // 終了コードを格納している変数文字列
            console.SetExitCodeStr(cmd);
        }
        public bool ExecCmd(object param)
        {
            // WebView2からのコールではasyncは即座にコントロールが返るため、
            // 前のコマンドの実行中に再度コールされる可能性がある。
            // 意図的な連続コマンド実行は配列で渡すことで対応できるため、
            // 前のコマンド実行中に再度本関数がコールされたら失敗として終了する。
            // 本関数自体はGUIスレッドからのみコールされるためフラグで排他制御できる。

            // コマンド実行拒否したらfalseを返す
            if (console.IsRunning)
            {
                return false;
            }

            // コマンド実行したらその結果に関わらずtrueを返す。
            // そもそも実行結果を確認する前にWebView2に処理が返る。
            // コマンド実行結果はExitCodeやPostWebMessageAsJsonによる通知で確認する。
            ExecCmdImpl(param);
            return true;
        }

        public async void ExecCmdImpl(object param)
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
