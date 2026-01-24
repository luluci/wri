using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace wri.Interface
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class Config
    {
        public string JsonText { get; set; } = null;
        public Interface.Json.Config Json;

        public Config()
        {
            // C#内で使用するconfig情報についてはJsonライブラリでC#クラス化する
            // WebView2用config情報は文字列だけ渡してWebView2内で処理する
        }

        public string GetConfig()
        {
            return JsonText;
        }
    }
}
