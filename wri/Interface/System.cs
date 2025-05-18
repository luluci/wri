using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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
            return console.Start();
        }
        public void Close()
        {
            console.Close();
        }

        public string[] Result
        {
            get
            {
                return console.Results.ToArray();
            }
        }

        public async void ExecCmdAsync(string cmd)
        {
            // JavaScriptから"async void func()"をコールすると
            // 関数実行開始したら即座に返る？
            await console.ExecCmdAsync(cmd);
            //
            var ss = new StringBuilder();
            foreach (string s in console.Results) {
                ss.Append(s);
            }
            var json = $"{{ \"result\" : [ {ss.ToString()} ] }}";
            GlobalData.WebView2.CoreWebView2.PostWebMessageAsJson(json);
        }

        public bool ExecCmd(string cmd)
        {
            return console.ExecCmd(cmd);
        }
        public bool ExecCmds(object param)
        {
            try
            {
                if (param is object[] ps)
                {
                    var list = ps.Select(x => x as string).ToArray();
                    return console.ExecCmd(list);
                }
                else
                {
                    var str = param as string;
                    return console.ExecCmd(str);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
