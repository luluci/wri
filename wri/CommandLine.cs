using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wri
{
    class CommandLine
    {
        // 
        Interface.EntryPoint ep = new Interface.EntryPoint();
        Type WinApiT = typeof(Interface.WindowsApi);
        // 解析状態
        enum OptionState
        {
            None,
            WinApi,
        }
        OptionState optionState = OptionState.None;

        public CommandLine()
        {
        }

        public void Parse(string[] args)
        {
            // コマンドライン引数をチェック
            foreach (string arg in args)
            {
                switch (arg)
                {
                    case "-h":
                    case "--help":
                        Help();
                        return;

                    case "--winapi":
                        optionState = OptionState.WinApi;
                        break;

                    default:
                        ParseValue(arg);
                        break;
                }
            }
        }

        private void ParseValue(string value)
        {
            switch (optionState)
            {
                case OptionState.WinApi:
                    ParseValueWinApi(value);
                    break;

                case OptionState.None:
                default:
                    // 読み捨て
                    break;
            }
        }

        private void ParseValueWinApi(string value)
        {
            try
            {
                // valueをメソッドとしてリフレクションを取得
                var method = WinApiT.GetMethod(value);
                if (!(method is null))
                {
                    method.Invoke(ep.win, null);
                }
                else
                {
                    Console.WriteLine(value + " is not function.");
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        public void Help()
        {
            Console.WriteLine("Usage: wri.exe <option> <value> <value> ... <option> <value> ...");
            Console.WriteLine("  option:");
            Console.WriteLine("    -h, --help : value = none");
            Console.WriteLine("    --winapi   : value = WindowsApiクラス内の実行したいメソッド名");
        }
    }
}
