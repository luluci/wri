using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    internal class Console
    {
        private Process process;
        private bool IsCmdRunning;
        private const string _promptMarker = "__CMD_COMPLETE__";  // コマンド完了検出用のマーカー
        private string _promptMarkerCmd;

        public List<string> Results;
        // コマンド実行用
        private string[] dummyCmds;

        public Console()
        {
            IsCmdRunning = false;
            Results = new List<string>();
            _promptMarkerCmd = $"echo {_promptMarker}";
            dummyCmds = new string[] {""};
        }

        public bool IsRunning
        {
            get
            {
                return IsCmdRunning;
            }
        }

        public bool Start()
        {
            try
            {
                if (!(process is null))
                {
                    process.Close();
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

        public void Close()
        {
            if (process != null)
            {
                process.Close();
            }
        }

        private void CmdProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                // コマンド完了を検出
                if (e.Data.Contains(_promptMarker))
                {
                    IsCmdRunning = false;
                }
                else
                {
                    Results.Add(e.Data);
                }
            }
        }

        Object lockCmd = new Object();
        public async Task ExecCmdAsync(string cmd)
        {
            dummyCmds[0] = cmd;
            await ExecCmdAsync(dummyCmds);
        }
        public async Task ExecCmdAsync(string[] cmds)
        {
            try
            {
                if (System.Threading.Monitor.TryEnter(lockCmd))
                {
                    ExecCmd(cmds);

                    await Task.Run(() =>
                    {
                        while (IsCmdRunning) Task.Delay(500);
                    });

                    System.Threading.Monitor.Exit(lockCmd);
                }
            }
            catch (Exception ex)
            {
            }
        }

        public bool ExecCmd(string cmd)
        {
            dummyCmds[0] = cmd;
            return ExecCmd(dummyCmds);
        }
        public bool ExecCmd(string[] cmds)
        {
            if (process is null)
            {
                return false;
            }
            if (IsCmdRunning)
            {
                return false;
            }

            try
            {
                IsCmdRunning = true;
                Results.Clear();
                foreach (var cmd in cmds)
                {
                    process.StandardInput.WriteLine(cmd);
                }
                process.StandardInput.WriteLine("echo ");
                process.StandardInput.WriteLine(_promptMarkerCmd);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
