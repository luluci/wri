using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wri.Log
{
    internal static class Logger
    {
        static public LoggerImpl Console { get; } = new LoggerImpl();
    }

    public delegate void LoggerPostProc();

    internal class LoggerImpl
    {
        public ReactiveCollection<string> Log { get; }

        public int LogMaxCount { get; set; } = 1000;

        public LoggerPostProc PostProc { get; set; } = () => {};

        public LoggerImpl()
        {
            Log = new ReactiveCollection<string>();
        }

        public void Add(string message)
        {
            if (Log.Count > LogMaxCount)
            {
                // ログが上限を超えたら古いログを削除
                Log.RemoveAt(0);
            }
            Log.Add(message);
            PostProc();
        }
        public void AddRange(IEnumerable<string> messages)
        {
            // ログを上限で削除するため、GUIスレッドで実行する必要がある
            if (Log.Count + messages.Count() > LogMaxCount)
            {
                // ログが上限を超えたら古いログを削除
                int removeCount = Log.Count + messages.Count() - LogMaxCount;
                for (int i = 0; i < removeCount; i++)
                {
                    Log.RemoveAt(0);
                }
            }
            Log.AddRangeOnScheduler(messages);
        }

        public void Clear()
        {
            Log.Clear();
        }
    }
}
