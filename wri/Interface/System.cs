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

        public async void ExecCmd(object param)
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
                //
                var ss = new StringBuilder();
                if (console.Results.Count > 0)
                {
                    ss.Append($"\"{console.Results[0]}\"");
                    for (int i = 1; i < console.Results.Count; i++)
                    {
                        ss.Append($", \"{console.Results[0]}\"");
                    }
                }
                var json = $"{{ \"result\" : [ {ss} ] }}";
                GlobalData.WebView2.CoreWebView2.PostWebMessageAsJson(json);
            }
            catch (Exception ex)
            {
                var json = $"{{ \"result\" : [ \"{ex.Message}\" ] }}";
                GlobalData.WebView2.CoreWebView2.PostWebMessageAsJson(json);
            }
        }
    }
}
