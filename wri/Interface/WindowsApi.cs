using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace wri.Interface
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class WindowsApi
    {
        // static, constはWebView2側から参照不可

        // I/F情報
        public string Error { get; set; } = string.Empty;

        // ユーザ情報
        public string MachineName;
        public string UserName;
        public string UserDomainName;

        // WindowsAPI
        public int WM_SYSCOMMAND = Utility.WindowsApi.WM_SYSCOMMAND;
        public int SC_MONITORPOWER = Utility.WindowsApi.SC_MONITORPOWER;
        public int MONITOR_OFF = Utility.WindowsApi.MONITOR_OFF;

        // EventLog
        static private int[] emptyFilter = { };
        // 最後に取得したログリスト
        public EventLog[] LatestGetEventLog = { };

        public WindowsApi()
        {
            MachineName = Environment.MachineName;
            UserName = Environment.UserName;
            UserDomainName = Environment.UserDomainName;
        }

        public int SendMessage(int hWnd, int hMsg, int wParam, int lParam)
        {
            return Utility.WindowsApi.SendMessage(hWnd, hMsg, wParam, lParam);
        }

        public void LockPC()
        {
            Utility.WindowsApi.LockWorkStation();
        }

        public void MonitorOff()
        {
            Utility.WindowsApi.SendMessage(-1, WM_SYSCOMMAND, SC_MONITORPOWER, MONITOR_OFF);
        }

        public EventLog[] GetEventLogList(string logName, string machineName, object[] filter, int log_max = 100)
        {
            int log_count = 0;
            bool log_flag = true;
            var logs = new List<EventLog>();

            bool enable_filter = false;
            var filter_dict = new Dictionary<int, bool>();
            if (filter.Length != 0)
            {
                foreach (var item in filter)
                {
                    try
                    {
                        filter_dict.Add((int)item, true);
                    }
                    catch
                    {

                    }
                }
                enable_filter = true;
            }

            try
            {
                if (System.Diagnostics.EventLog.Exists(logName, machineName))
                {
                    using (var log = new System.Diagnostics.EventLog(logName, machineName))
                    {
                        int max = log.Entries.Count;
                        for (int idx = max - 1; idx >= 0; idx--)
                        //foreach (System.Diagnostics.EventLogEntry entry in log.Entries)
                        {
                            var entry = log.Entries[idx];

                            // filter
                            log_flag = true;
                            if (enable_filter)
                            {
                                if (!filter_dict.ContainsKey((int)entry.InstanceId))
                                {
                                    log_flag = false;
                                }
                            }

                            if (log_flag)
                            {
                                logs.Add(new EventLog
                                {
                                    InstanceId = entry.InstanceId,
                                    Message = entry.Message,
                                    TimeGenerated = new DateTime(entry.TimeGenerated),
                                });
                                log_count++;

                                if (log_count >= log_max)
                                {
                                    break;
                                }
                            }

                        }
                    }
                }

                Error = string.Empty;
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }

            LatestGetEventLog = logs.ToArray();
            return LatestGetEventLog;
        }

        public EventLog[] GetLogOnOffEventLogList(string machineName, int log_max = 100)
        {
            var logName = "System";
            int log_count = 0;
            bool log_flag = true;
            string message;
            var logs = new List<EventLog>();

            try
            {
                if (System.Diagnostics.EventLog.Exists(logName, machineName))
                {
                    using (var log = new System.Diagnostics.EventLog(logName, machineName))
                    {
                        int max = log.Entries.Count;
                        for (int idx = max-1; idx>=0; idx--)                        
                        //foreach (System.Diagnostics.EventLogEntry entry in log.Entries)
                        {
                            var entry = log.Entries[idx];
                            log_flag = true;
                            switch (entry.InstanceId)
                            {
                                case 6005:
                                    // ログオン:起動
                                    message = "ログオン:起動";
                                    break;

                                case 6006:
                                    // ログオフ:正常シャットダウン
                                    message = "ログオフ:正常シャットダウン";
                                    break;

                                case 6008:
                                    // ログオフ:正常ではないシャットダウン
                                    message = "ログオフ:正常ではないシャットダウン";
                                    break;

                                case 6009:
                                    // ログオン:起動時にブート情報を記録
                                    message = "ログオン:起動時にブート情報を記録";
                                    break;

                                case 1:
                                    // ログオフ:スリープ
                                    message = "ログオフ:スリープ";
                                    break;

                                case 42:
                                    // ログオン:スリープから復帰
                                    message = "ログオン:スリープから復帰";
                                    break;

                                case 12:
                                    // ログオン:OS起動
                                    message = "ログオン:OS起動";
                                    break;

                                case 13:
                                    // ログオン:起動
                                    message = "ログオン:起動";
                                    break;

                                case 7001:
                                    // ログオン:起動
                                    message = "ログオン:起動";
                                    break;

                                case 7002:
                                    // ログオフ:シャットダウン
                                    message = "ログオフ:シャットダウン";
                                    break;

                                default:
                                    log_flag = false;
                                    message = string.Empty;
                                    break;
                            }


                            if (log_flag)
                            {
                                logs.Add(new EventLog
                                {
                                    InstanceId = entry.InstanceId,
                                    Message = message,
                                    TimeGenerated = new DateTime(entry.TimeGenerated),
                                });
                                log_count++;

                                if (log_count >= log_max)
                                {
                                    break;
                                }
                            }

                        }
                    }
                }

                Error = string.Empty;
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }

            LatestGetEventLog = logs.ToArray();
            return LatestGetEventLog;
        }
    }


    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class EventLog
    {
        public long InstanceId { get; set; }
        public string Message { get; set; }
        public DateTime TimeGenerated { get; set; }
    }

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class DateTime
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Second { get; set; }
        public int Millisecond { get; set; }

        public DateTime(System.DateTime dt)
        {
            Year = dt.Year;
            Month = dt.Month;
            Day = dt.Day;
            Hour = dt.Hour;
            Minute = dt.Minute;
            Second = dt.Second;
            Millisecond = dt.Millisecond;
        }
    }
}
