using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static Utility.Console;

namespace Utility
{
    internal class Console
    {
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
        // コマンド実行用
        private string[] dummyCmds;

        public Console()
        {
            IsCmdRunning = false;
            _promptMarkerCmd = $"echo {_promptMarker} {_exitCodeCmd}";
            _promptMarkerRegex = new Regex($@"^{_promptMarker} (.+)");
            dummyCmds = new string[] {""};
        }

        public bool IsRunning
        {
            get
            {
                return IsCmdRunning;
            }
        }

        public delegate void CallbackStdout(string output);
        public delegate void CallbackExit(int code);
        private CallbackStdout callbackStdout;
        private CallbackExit callbackExit;

        public bool Start(CallbackStdout stdout, CallbackExit exit)
        {
            try
            {
                callbackStdout = stdout;
                callbackExit = exit;

                if (!(process is null))
                {
                    process.Close();
                    _promptMarkerCmd = $"echo {_promptMarker} {_exitCodeCmd}";
                    ExitCode = 0;
                    ErrorMessage = string.Empty;
                }

                process = new Process();
                process.StartInfo.FileName = "cmd.exe";
                //Console.StartInfo.Arguments = "--login -i";
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                process.OutputDataReceived += CmdProcess_OutputDataReceived;
                process.ErrorDataReceived += CmdProcess_OutputDataReceived;
                process.Start();
                process.StandardInput.WriteLine("@echo off");
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
            catch
            {
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
                process.Close();
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
