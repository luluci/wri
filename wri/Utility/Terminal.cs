using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Utility
{
    /// <summary>
    /// 環境設定をクラスで管理してコマンドは都度Processで実行して管理するクラス
    /// </summary>
    public class Terminal
    {
        // プロセス情報
        public int ProcessId { get; internal set; } = -1;
        //
        private Process process;
        private ProcessStartInfo psi;
        private DataReceivedEventHandler stdout;
        private DataReceivedEventHandler stderr;
        private EventHandler exit;

        // 実行結果
        public string ErrorMessage = string.Empty;

        public Terminal()
        {
        }

        public void Init(string encoding = null, DataReceivedEventHandler stdout_ = null, DataReceivedEventHandler stderr_ = null, EventHandler exit_ = null)
        {
            this.Init(encoding.ToEncoding(), stdout_, stderr_, exit_);
        }
        public void Init(Encoding encoding = null, DataReceivedEventHandler stdout_ = null, DataReceivedEventHandler stderr_ = null, EventHandler exit_ = null)
        {
            // Processを実行していたら解放
            Close();
            // 状態初期化
            ErrorMessage = string.Empty;
            // Process初期化
            psi = new ProcessStartInfo();
            //psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = !(stdout_ is null);
            psi.RedirectStandardError = !(stderr_ is null);
            psi.StandardOutputEncoding = encoding ?? Encoding.UTF8;
            psi.StandardErrorEncoding = encoding ?? Encoding.UTF8;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            stdout = stdout_;
            stderr = stderr_;
            exit = exit_;
        }

        public void Close()
        {
            if (process != null)
            {
                if (!process.HasExited)
                {
                    process.Kill();
                    process.WaitForExit(10 * 1000);
                }
                process.Dispose();
                process = null;
                ProcessId = -1;
            }
        }

        public bool HasExited
        {
            get
            {
                if (process == null)
                {
                    return true;
                }
                return process.HasExited;
            }
        }
        public int ExitCode
        {
            get
            {
                if (process == null || !process.HasExited)
                {
                    return -1;
                }
                return process.ExitCode;
            }
        }

        public void ClearEnv()
        {
            psi.EnvironmentVariables.Clear();
        }
        public void ClearEnv(string key)
        {
            if (psi.EnvironmentVariables.ContainsKey(key))
            {
                psi.EnvironmentVariables.Remove(key);
            }
        }
        public void SetEnv(string key, string value)
        {
            if (psi.EnvironmentVariables.ContainsKey(key))
            {
                psi.EnvironmentVariables[key] = value;
            }
            else
            {
                psi.EnvironmentVariables.Add(key, value);
            }
        }
        public void AddEnv(string key, string value)
        {
            if (psi.EnvironmentVariables.ContainsKey(key))
            {
                psi.EnvironmentVariables[key] = value + ";" + psi.EnvironmentVariables[key];
            }
            else
            {
                psi.EnvironmentVariables.Add(key, value);
            }
        }

        public void SetWorkingDirectory(string path)
        {
            psi.WorkingDirectory = path;
        }
        public void ChangeWorkingDirectory(string path)
        {
            if (string.IsNullOrEmpty(psi.WorkingDirectory) || !path.StartsWith("."))
            {
                psi.WorkingDirectory = path;
            }
            else
            {
                var work = System.IO.Path.Combine(psi.WorkingDirectory, path);
                psi.WorkingDirectory = System.IO.Path.GetFullPath(work);
            }
        }

        public void SetEncoding(string encoding)
        {
            SetEncoding(encoding.ToEncoding());
        }
        public void SetEncoding(Encoding encoding)
        {
            psi.StandardOutputEncoding = encoding;
        }

        public bool ExecCmd(string filename, string args)
        {
            try
            {
                psi.FileName = filename;
                psi.Arguments = args;

                process = new Process
                {
                    StartInfo = psi
                };
                //process.StandardInput.NewLine = "\n";
                if (!(stdout is null)) process.OutputDataReceived += stdout;
                if (!(stderr is null)) process.ErrorDataReceived += stderr;
                if (!(exit is null))
                {
                    process.EnableRaisingEvents = true;
                    process.Exited += exit;
                }
                process.Start();
                if (!(stdout is null)) process.BeginOutputReadLine();
                if (!(stderr is null)) process.BeginErrorReadLine();
                ProcessId = process.Id;
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                ProcessId = -1;
                return false;
            }
        }
    }
}
