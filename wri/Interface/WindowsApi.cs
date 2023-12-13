using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace wri.Interface
{
    using UtilWinApi = Utility.WindowsApi;

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
        public int WM_SYSCOMMAND = UtilWinApi.WM_SYSCOMMAND;
        public int SC_MONITORPOWER = UtilWinApi.SC_MONITORPOWER;
        public int MONITOR_OFF = UtilWinApi.MONITOR_OFF;

        // hWndリスト
        //private List<IntPtr> EnumWindowsList = new List<IntPtr>();

        // 一時バッファ
        // WINDOWPLACEMENT
        private UtilWinApi.WINDOWPLACEMENT windowPlacement = new UtilWinApi.WINDOWPLACEMENT();
        private LinkedList<WindowInfo> enumActiveWindowsList = new LinkedList<WindowInfo>();
        private StringBuilder sbWindowText = new StringBuilder(256);

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


        // 速度が気になったとき用
        // JavaScriptはシングルスレッドで動くので、JavaScript連携前提であれば単一スレッドからのみアクセスされる想定。
        // 一応少しでも高速化するためにdelegateインスタンスをキャッシュするときにはstaticである必要がある。
        //private static LinkedList<IntPtr> EnumWindowsList = new LinkedList<IntPtr>();
        //private UtilWinApi.EnumWindowCallBack delegateEnumWindowsProc = new UtilWinApi.EnumWindowCallBack(EnumWindowsProc);

        public IntPtr[] EnumWindows()
        {
            var list = new LinkedList<IntPtr>();
            UtilWinApi.EnumWindows(
                delegate (IntPtr hWnd, IntPtr lParam) {
                    list.AddLast(hWnd);
                    return true;
                },
                IntPtr.Zero
            );
            return list.ToArray();
        }

        public int GetMonitorCount()
        {
            return UtilWinApi.GetSystemMetrics(UtilWinApi.SystemMetric.SM_CMONITORS);
        }

        public void GetDisplayInfo()
        {
            foreach (var s in global::System.Windows.Forms.Screen.AllScreens)
            {
                Console.WriteLine("{0} {1}", s.DeviceName, s.Bounds);
            }
        }

        public void MoveWindowIntoScreen()
        {
            // ウインドウを取得
            enumActiveWindowsList.Clear();
            UtilWinApi.EnumWindows(EnumActiveWindowProc, IntPtr.Zero);
            //
            foreach (var info in enumActiveWindowsList)
            {
                if (info.ScreenId == -1)
                {
                    //UtilWinApi.MoveWindow(info.hWnd, info.NearScreenX, info.NearScreenY, info.Width, info.Height, 1);
                    UtilWinApi.SetWindowPos(info.hWnd, IntPtr.Zero, info.NearScreenX, info.NearScreenY, 0, 0, UtilWinApi.SWP_FLAG_MOVEONLY);
                }
            }
        }

        public WindowInfo[] EnumActiveWindowInfo()
        {
            // ディスプレイ情報を取得
            // System.Windows.Forms.Screen.AllScreensを変数に取り込む意味ない？
            // ウインドウを取得
            enumActiveWindowsList.Clear();
            UtilWinApi.EnumWindows(EnumActiveWindowProc, IntPtr.Zero);
            return enumActiveWindowsList.ToArray();
        }
        bool EnumActiveWindowProc(IntPtr hWnd, IntPtr lParam)
        {
            //string hasParent = "";
            //if (UtilWinApi.GetParent(hWnd) > 0)
            //    hasParent = "Has Parent";
            //else
            //    hasParent = "";
            //StringBuilder sbClassName = new StringBuilder(256);
            //UtilWinApi.GetClassName(hWnd, sbClassName, sbClassName.Capacity);
            //sbClassName.Length > 0

            // WinAPI呼びまくりになるので、条件不成立で即returnする
            // 不可視ウインドウは対象外
            if (!(UtilWinApi.IsWindowVisible(hWnd) > 0))
            {
                return true;
            }
            // ドラッグで移動可能なウインドウのみ対象
            if (!IsMovableWindow(hWnd))
            {
                return true;
            }
            // 表示されているウインドウのみ対象
            if (UtilWinApi.GetWindowPlacement(hWnd, ref windowPlacement))
            {
                // OK
            }
            else
            {
                return true;
            }

            // 条件成立でウインドウ情報を登録する
            var info = new WindowInfo
            {
                hWnd = hWnd,
            };
            // タイトルバー表示を取得
            sbWindowText.Clear();
            UtilWinApi.GetWindowText(hWnd, sbWindowText, sbWindowText.Capacity);
            info.Title = sbWindowText.ToString();
            //
            info.ShowState = windowPlacement.showCmd;
            switch (windowPlacement.showCmd)
            {
                case UtilWinApi.SW_SHOWNORMAL:
                case UtilWinApi.SW_SHOWMAXIMIZED:
                    // OK
                    // ウインドウ座標取得
                    info.X = windowPlacement.rcNormalPosition.X;
                    info.Y = windowPlacement.rcNormalPosition.Y;
                    info.Width = Math.Abs(windowPlacement.rcNormalPosition.Width);
                    info.Height = Math.Abs(windowPlacement.rcNormalPosition.Height);
                    // ディスプレイ情報取得
                    // ptMaxPositionは常に(-1,-1)なので使えない
                    // 最大化時も通常時座標が配置されているディスプレイ基準になっているのでそちらを使う
                    CheckScreenInfo(info, windowPlacement.rcNormalPosition);
                    break;
                    
                default:
                    return true;
            }
            //
            enumActiveWindowsList.AddLast(info);
            
            return true;
        }
        public void UpdateScreenInfo(WindowInfo info, int id, global::System.Windows.Forms.Screen screen, global::System.Drawing.Point pt)
        {
            info.ScreenId = id;
            info.DisplayDeviceName = screen.DeviceName;
            //
            info.ScreenX = pt.X - screen.Bounds.X;
            info.ScreenY = pt.Y - screen.Bounds.Y;
        }
        public void CheckScreenInfo(WindowInfo info, global::System.Drawing.Rectangle rect)
        {
            int screen_id = 0;
            int near_screen_diff = int.MaxValue;
            int near_screen_id = -1;

            foreach (var s in global::System.Windows.Forms.Screen.AllScreens)
            {
                // Rectangle
                if (s.Bounds.Contains(rect.Location))
                {
                    // 共通処理
                    UpdateScreenInfo(info, screen_id, s, rect.Location);
                    return;
                }
                else
                {
                    // 一番近いスクリーンを記憶しておく
                    var diff = Math.Abs(rect.Location.X - s.Bounds.X) + Math.Abs(rect.Location.Y - s.Bounds.Y);
                    if (diff < near_screen_diff)
                    {
                        near_screen_diff = diff;
                        near_screen_id = screen_id;
                    }
                }

                screen_id++;
            }

            // 所属スクリーンなし
            info.ScreenId = -1;
            // 近傍スクリーン記憶
            info.NearScreenId = near_screen_id;
            info.NearScreenX = global::System.Windows.Forms.Screen.AllScreens[near_screen_id].Bounds.X;
            info.NearScreenY = global::System.Windows.Forms.Screen.AllScreens[near_screen_id].Bounds.Y;
        }

        public void EnumWindow2()
        {
            var placement = new UtilWinApi.WINDOWPLACEMENT();
            placement.length = Marshal.SizeOf(placement);

            foreach (var process in global::System.Diagnostics.Process.GetProcesses())
            {
                IntPtr hWnd = process.MainWindowHandle;
                if (hWnd != IntPtr.Zero)
                {
                    bool isMouseMovable = IsMovableWindow(hWnd);
                    if (isMouseMovable)
                    {
                        Console.WriteLine(process.ProcessName + " : " + process.MainWindowTitle + " => Movable");
                    }
                    else
                    {
                        Console.WriteLine(process.ProcessName + " : " + process.MainWindowTitle + " => not Movable");
                    }


                    UtilWinApi.EnumChildWindows(hWnd, EnumChildProc, IntPtr.Zero);


                }
            }
            // 出力例：
            // vim : VIM - C:\c#\tips\enumwin\enumwin.cs
            // explorer : C:\bin
            // NetCaptor : NetCaptor
            // OUTLOOK : 予定表 - Microsoft Outlook
            // cmd : コマンド プロンプト - enumwin
            // iexplore : ＠IT：Insider.NET - Microsoft Internet Explorer
        }

        bool EnumChildProc(IntPtr hWnd, IntPtr lParam)
        {
            // ここで子ウィンドウの操作を行う
            string windowText = UtilWinApi.GetWindowText(hWnd);

            bool isMouseMovable = IsMovableWindow(hWnd);
            if (isMouseMovable)
            {
                Console.WriteLine(windowText + " => Movable");
            }
            else
            {
                Console.WriteLine(windowText + " => not Movable");
            }

            return true; // true を返すことで列挙を継続
        }

        bool IsMovableWindow(IntPtr hWnd)
        {
            int windowStyle = UtilWinApi.GetWindowLong(hWnd, UtilWinApi.GWL_STYLE);
            int exWindowStyle = UtilWinApi.GetWindowLong(hWnd, UtilWinApi.GWL_EXSTYLE);

            return (windowStyle & UtilWinApi.WS_CAPTION) != 0 && (exWindowStyle & UtilWinApi.WS_EX_TOOLWINDOW) == 0;
        }
        bool IsVisibleWindow(IntPtr hWnd)
        {
            if (UtilWinApi.GetWindowPlacement(hWnd, ref windowPlacement))
            {
                if (windowPlacement.showCmd == UtilWinApi.SW_SHOWNORMAL || windowPlacement.showCmd == UtilWinApi.SW_SHOWMAXIMIZED)
                {
                    // 通常ウインドウ or 最大化ウインドウ
                    return true;
                    //Console.WriteLine("対象ウィンドウはデスクトップ上に表示されています。");
                    //Console.WriteLine(process.ProcessName + " : " + process.MainWindowTitle + " => displayed");
                }
                else if (windowPlacement.showCmd == UtilWinApi.SW_SHOWMINIMIZED)
                {
                    // 最小化ウインドウ
                    return false;
                    //Console.WriteLine("対象ウィンドウは最小化されています。");
                    //Console.WriteLine(process.ProcessName + " : " + process.MainWindowTitle + " => minimamed");
                }
                else
                {
                    // 状態不明
                    return false;
                    //Console.WriteLine("対象ウィンドウの表示状態は不明です。");
                    //Console.WriteLine(process.ProcessName + " : " + process.MainWindowTitle + " => not found");
                }
            }

            return false;
        }


        public int SendMessage(int hWnd, int hMsg, int wParam, int lParam)
        {
            return UtilWinApi.SendMessage(hWnd, hMsg, wParam, lParam);
        }

        public void LockPC()
        {
            UtilWinApi.LockWorkStation();
        }

        public void MonitorOff()
        {
            UtilWinApi.SendMessage(-1, WM_SYSCOMMAND, SC_MONITORPOWER, MONITOR_OFF);
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
                if (global::System.Diagnostics.EventLog.Exists(logName, machineName))
                {
                    using (var log = new global::System.Diagnostics.EventLog(logName, machineName))
                    {
                        int max = log.Entries.Count;
                        for (int idx = max - 1; idx >= 0; idx--)
                        //foreach (System.Diagnostics.EventLogEntry entry in log.Entries)
                        {
                            var entry = log.Entries[idx];
                            var event_id = entry.InstanceId & 0xFFFF;

                            // filter
                            log_flag = true;
                            if (enable_filter)
                            {
                                if (!filter_dict.ContainsKey((int)event_id))
                                {
                                    log_flag = false;
                                }
                            }

                            if (log_flag)
                            {
                                logs.Add(new EventLog
                                {
                                    InstanceId = entry.InstanceId,
                                    EventId = event_id,
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
                if (global::System.Diagnostics.EventLog.Exists(logName, machineName))
                {
                    using (var log = new global::System.Diagnostics.EventLog(logName, machineName))
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
    public class WindowInfo
    {
        public IntPtr hWnd { get; set; }
        public string Title { get; set; }
        public int ShowState { get; set; }
        // ウインドウ位置情報
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        // ディスプレイ情報
        public int ScreenId { get; set; }
        public string DisplayDeviceName { get; set; }
        // screen座標を基準にした相対位置
        public int ScreenX { get; set; }
        public int ScreenY { get; set; }
        // 近傍スクリーン情報
        public int NearScreenId { get; set; }
        public int NearScreenX { get; set; }
        public int NearScreenY { get; set; }
    }

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class EventLog
    {
        public long InstanceId { get; set; }
        public long EventId { get; set; }
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

        public DateTime(global::System.DateTime dt)
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
