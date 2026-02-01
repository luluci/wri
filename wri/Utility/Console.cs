using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using wri.Interface;
using static Utility.Console;

namespace Utility
{
    using winapi = Utility.WindowsApi;

    internal class Console
    {
        // プロセス情報
        public int ProcessId
        {
            get
            {
                if (process != null && !process.HasExited)
                {
                    return process.Id;
                }
                else
                {
                    return -1;
                }
            }
        }
        // 実行結果
        public int ExitCode = 0;
        public string ErrorMessage = string.Empty;
        //
        private Process process;
        private bool IsCmdRunning;
        private const string _promptMarker = "__CMD_COMPLETE__";
        private const string _exitCodeCmd = "%ERRORLEVEL%";
        private const string _exitCodeBash = "$?";
        private string _exitCodeStr = string.Empty;
        private string _promptMarkerCmd;
        private Regex _promptMarkerRegex;
        private Dictionary<string, string> _env;
        // コマンド実行用
        private string[] dummyCmds;

        public Console()
        {
            Init();
        }

        public void Init()
        {
            ExitCode = 0;
            ErrorMessage = string.Empty;
            IsCmdRunning = false;
            _promptMarkerCmd = $"echo {_promptMarker} {_exitCodeCmd}";
            _promptMarkerRegex = new Regex($@"^{_promptMarker} (.+)");
            _env = new Dictionary<string, string>();
            dummyCmds = new string[] { "" };
        }

        public bool IsRunning
        {
            get
            {
                return IsCmdRunning;
            }
        }

        // コマンド実行
        public delegate void CallbackExec(string output);
        // 標準出力取得
        public delegate void CallbackStdout(string output);
        // 終了検知
        public delegate void CallbackExit(int code);
        private CallbackExec callbackExec;
        private CallbackStdout callbackStdout;
        private CallbackExit callbackExit;

        public void ClearEnv()
        {
            _env.Clear();
        }
        public void SetEnv(string key, string value)
        {
            if (_env.ContainsKey(key))
            {
                _env[key] = value;
            }
            else
            {
                _env.Add(key, value);
            }
        }

        public bool StartCmd(CallbackExec exec, CallbackStdout stdout, CallbackExit exit)
        {
            try
            {
                //Console.StartInfo.Arguments = "--login -i";
                var result = Start("cmd.exe", "", exec, stdout, exit);
                if (result)
                {
                    process.StandardInput.WriteLine("@echo off");
                }
                return result;
            }
            catch
            {
                return false;
            }
        }
        public bool StartMsys2Ucrt64(CallbackExec exec, CallbackStdout stdout, CallbackExit exit)
        {
            string cmd = ".\\msys2_shell.cmd";
            string arg = " -ucrt64 -defterm -no-start -here";

            //if (Start(stdout, exit))
            //{
            //    var msys2Ucrt64Cmds = new string[] {
            //        "set PATH=C:\\msys64\\ucrt64\\bin;C:\\msys64\\usr\\local\\bin;C:\\msys64\\usr\\bin;C:\\msys64\\bin;%PATH%",
            //        "set MSYSTEM=UCRT64",
            //        "bash"
            //    };
            //    return ExecCmd(msys2Ucrt64Cmds);
            //}
            return Start(cmd, arg, exec, stdout, exit);
        }
        public bool Start(string filename, string args, CallbackExec exec, CallbackStdout stdout, CallbackExit exit)
        {
            try
            {
                callbackExec = exec;
                callbackStdout = stdout;
                callbackExit = exit;

                if (!(process is null))
                {
                    Close();
                }

                process = new Process();
                process.StartInfo.FileName = filename;
                process.StartInfo.Arguments = args;
                // 環境変数設定
                foreach (var kvp in _env)
                {
                    if (process.StartInfo.EnvironmentVariables.ContainsKey(kvp.Key))
                    {
                        process.StartInfo.EnvironmentVariables[kvp.Key] = kvp.Value + ";" + process.StartInfo.EnvironmentVariables[kvp.Key];
                    }
                    else
                    {
                        process.StartInfo.EnvironmentVariables.Add(kvp.Key, kvp.Value);
                    }
                }
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                process.OutputDataReceived += CmdProcess_OutputDataReceived;
                process.ErrorDataReceived += CmdProcess_OutputDataReceived;
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // msys2を起動するサンプル
                // ucrt64でmsys2を起動
                //var msys2Ucrt64Cmds = new string[] {
                //    "set PATH=C:\\msys64\\ucrt64\\bin;C:\\msys64\\usr\\local\\bin;C:\\msys64\\usr\\bin;C:\\msys64\\bin;%PATH%",
                //    "set MSYSTEM=UCRT64",
                //    "bash"
                //};
                //ExecCmd(msys2Ucrt64Cmds);
                // mingw64でmsys2を起動
                //var msys2Mingw64Cmds = new string[] {
                //    "set PATH=C:\\msys64\\mingw64\\bin;C:\\msys64\\usr\\local\\bin;C:\\msys64\\usr\\bin;C:\\msys64\\bin;%PATH%",
                //    "set MSYSTEM=MINGW64",
                //    "bash"
                //};
                //ExecCmd(msys2Mingw64Cmds);

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }
        }

        public void SetExitCodeStr(string cmd)
        {
            _exitCodeStr = cmd;
            _promptMarkerCmd = $"echo {_promptMarker} {_exitCodeStr}";
        }
        public void SetBash()
        {
            _promptMarkerCmd = $"echo {_promptMarker} {_exitCodeBash}";
        }

        public void Close()
        {
            if (process != null)
            {
                if (!process.HasExited)
                {
                    process.Kill();
                    process.WaitForExit(10*1000);
                }
                process.Close();
                process = null;
                Init();
            }
        }

        public bool SendCtrlC()
        {
            try
            {
                if (process != null && !process.HasExited)
                {
                    //if (winapi.AttachConsole((int)process.Id))
                    //{
                    //    // 2. Ctrl+Cを送信
                    //    winapi.GenerateConsoleCtrlEvent(winapi.CTRL_C_EVENT, 0);

                    //    // 3. コンソールをデタッチ
                    //    winapi.FreeConsole();
                    //}
                    //process.StandardInput.WriteLine("\x3");
                    winapi.GenerateConsoleCtrlEvent(winapi.CTRL_C_EVENT, (uint)process.Id);
                }
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }
        }

        private void CmdProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            //if (!string.IsNullOrWhiteSpace(e.Data))
            //{
            //}
            // コマンド完了を検出
            if (!(e.Data is null))
            {
                var match = _promptMarkerRegex.Match(e.Data);
                if (match.Success)
                {
                    if (!int.TryParse(match.Groups[1].Value, out ExitCode))
                    {
                        ExitCode = -1;
                    }
                    callbackExit(ExitCode);
                    IsCmdRunning = false;
                }
                else
                {
                    callbackStdout(e.Data);
                }
            }
        }

        public async Task<bool> ExecCmdAsync(string cmd)
        {
            dummyCmds[0] = cmd;
            return await ExecCmdAsync(dummyCmds);
        }
        public async Task<bool> ExecCmdAsync(string[] cmds)
        {
            bool result = false;
            // コマンド実行をリジェクトした場合にfalseを返す。
            // コマンド実行したら結果に関わらずtrueを返す。
            try
            {
                if (IsCmdRunning)
                {
                    return false;
                }
                IsCmdRunning = true;

                if (ExecCmd(cmds))
                {
                    result = true;
                    await Task.Run(() =>
                    {
                        while (IsCmdRunning) Task.Delay(500);
                    });
                }
                else
                {
                    ErrorMessage = "ExecCmd() failed.";
                    ExitCode = -1;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                ExitCode = -1;
            }
            finally
            {
                IsCmdRunning = false;
            }

            return result;
        }

        public bool ExecCmd(string cmd)
        {
            dummyCmds[0] = cmd;
            return ExecCmd(dummyCmds);
        }
        public bool ExecCmd(string[] cmds)
        {
            try
            {
                if (process is null)
                {
                    ErrorMessage = "process is not initialized.";
                    return false;
                }
                // 入力されたコマンドをすべてstdinに流す
                foreach (var cmd in cmds)
                {
                    callbackExec(cmd);
                    process.StandardInput.WriteLine(cmd);
                }
                // コマンド実行完了を検知するためのダミーコマンド実行
                process.StandardInput.WriteLine(_promptMarkerCmd);
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }
        }
    }
}
