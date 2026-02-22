using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Utility;

namespace wri.Interface
{
    using UtilWinApi = global::Utility.WindowsApi;
    using Console = global::Utility.Console;

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class SystemIf
    {
        public ConsoleIf Console { get; set; }
        public ProcessIf Process { get; set; }

        public SystemIf()
        {
            Console = new ConsoleIf();
            Process = new ProcessIf();
        }

        public void GC()
        {
            System.GC.Collect();
        }

        public TerminalIf CreateTerminal()
        {
            return new TerminalIf();
        }

        public void AddLog(string msg)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                Log.Logger.Console.Add(msg);
            }));
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

            var startInfo = new System.Diagnostics.ProcessStartInfo()
            {
                FileName = path,
                UseShellExecute = true,
                CreateNoWindow = true,
                Arguments = arguments,
            };
            System.Diagnostics.Process.Start(startInfo);
        }
        public void Kill(int pid)
        {
            try
            {
                var process = global::System.Diagnostics.Process.GetProcessById(pid);
                process.Kill();
            }
            catch (Exception)
            {
                // Handle exception if needed
            }
        }
    }

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class TerminalIf
    {
        Terminal terminal;
        int buffsize = 100;
        List<List<string>> stdoutBuff = new List<List<string>>();
        int tgtBuff = 0;

        public bool TransferStdoutToWebView2 { get; set; }

        public bool HasExited
        {
            get { return terminal.HasExited; }
        }
        public int ExitCode
        {
            get { return terminal.ExitCode; }
        }
        public string ErrorMessage
        {
            get { return terminal.ErrorMessage; }
        }
        public int ProcessId
        {
            get { return terminal.ProcessId; }
        }
        public TerminalIf()
        {
            terminal = new Terminal();
            stdoutBuff.Add(new List<string>(buffsize));
            stdoutBuff.Add(new List<string>(buffsize));
            tgtBuff = 0;

            Init();
        }
        public void Init(string encoding = null)
        {
            terminal.Init(encoding, OnStdout, OnStdout, OnExit);
        }
        public void Close()
        {
            terminal.Close();
        }

        internal void OnStdout(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            if (!(e.Data is null))
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    if (TransferStdoutToWebView2)
                    {
                        var json = Json.MakeJsonStringConsoleStdout(e.Data);
                        GlobalData.WebView2.CoreWebView2.PostWebMessageAsJson(json);
                    }
                    else
                    {
                        Log.Logger.Console.Add(e.Data);
                    }
                }));
            }
        }
        internal void OnExit(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                if (TransferStdoutToWebView2)
                {
                    //ExitCode = console.ExitCode;
                }
                else
                {
                    //Log.Logger.Console.Add($"< Exit (Result: {terminal.ExitCode})");
                }
                var json = Json.MakeJsonStringTerminalExit(terminal.ProcessId, ExitCode);
                GlobalData.WebView2.CoreWebView2.PostWebMessageAsJson(json);
            }));
        }

        public void AddEnv(string key, string value)
        {
            terminal.AddEnv(key, value);
        }
        public void SetEnv(string key, string value)
        {
            terminal.SetEnv(key, value);
        }
        public void SetWorkingDirectory(string path)
        {
            terminal.SetWorkingDirectory(path);
        }
        public void ChangeWorkingDirectory(string path)
        {
            terminal.ChangeWorkingDirectory(path);
        }

        public void SetEncoding(string encoding)
        {
            terminal.SetEncoding(encoding);
        }

        public bool ExecCmd(string cmd, string args, bool transStdout = false)
        {
            TransferStdoutToWebView2 = transStdout;
            return terminal.ExecCmd(cmd, args);
        }
    }

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class ConsoleIf
    {
        Console console;

        public bool TransferStdoutToWebView2 { get; set; }

        public int ProcessId
        {
            get { return console.ProcessId; }
        }

        public ConsoleIf()
        {
            console = new Console();

            TransferStdoutToWebView2 = false;
        }

        public void ClearEnv()
        {
            console.ClearEnv();
        }
        public void SetEnv(string key, string value)
        {
            console.SetEnv(key, value);
        }

        public bool Start(string cmd, string args, bool transStdout = false)
        {
            TransferStdoutToWebView2 = transStdout;
            if (!transStdout)
            {
                Interface.GlobalData.SetFooterVisibility(true);
            }
            return console.Start(cmd, args, OnExec, OnStdout, OnExit);
        }
        public bool StartCmd(bool transStdout = false)
        {
            TransferStdoutToWebView2 = transStdout;
            if (!transStdout)
            {
                Interface.GlobalData.SetFooterVisibility(true);
            }
            return console.StartCmd(OnExec, OnStdout, OnExit);
        }
        public void SetBash()
        {
            console.SetBash();
        }
        public void Close()
        {
            console.Close();
        }

        private void OnExec(string cmd)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                if (TransferStdoutToWebView2)
                {
                    //var json = Json.MakeJsonStringConsoleExec(output);
                    //GlobalData.WebView2.CoreWebView2.PostWebMessageAsJson(json);
                }
                else
                {
                    Log.Logger.Console.Add($"> {cmd}");
                }
            }));
        }
        private void OnStdout(string stdout)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                if (TransferStdoutToWebView2)
                {
                    var json = Json.MakeJsonStringConsoleStdout(stdout);
                    GlobalData.WebView2.CoreWebView2.PostWebMessageAsJson(json);
                }
                else
                {
                    Log.Logger.Console.Add(stdout);
                }
            }));
        }
        private void OnExit(int code)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                if (TransferStdoutToWebView2)
                {
                    //ExitCode = console.ExitCode;
                }
                else
                {
                    Log.Logger.Console.Add($"< Exit (Result: {code})");
                }
            }));
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
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                ExitCode = -1;
            }
            finally
            {
                var json = Json.MakeJsonStringConsoleExit(ExitCode);
                GlobalData.WebView2.CoreWebView2.PostWebMessageAsJson(json);
            }
        }

        public void SendCtrlC()
        {
            console.SendCtrlC();
        }
    }
}
