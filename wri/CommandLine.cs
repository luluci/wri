using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace wri
{
    public class CommandLine
    {
        public bool IsGuiMode;

        // GUIモード
        public string InputFile;

        // コンソールモード
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
            IsGuiMode = true;
            InputFile = string.Empty;
        }

        public void Parse(string[] args)
        {
            if (args.Length > 0)
            {
                var ext = System.IO.Path.GetExtension(args[0]);
                switch (ext)
                {
                    case ".html":
                        InputFile = args[0];
                        break;

                    default:
                        break;
                }
            }

            // コマンドライン引数をチェック
            foreach (string arg in args)
            {
                switch (arg)
                {
                    case "-h":
                    case "--help":
                        IsGuiMode = false;
                        Help();
                        return;

                    case "--winapi":
                        IsGuiMode = false;
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
            Console.WriteLine("Usage:");
            Console.WriteLine("  [GUI Mode]    wri.exe <html file>");
            Console.WriteLine("  [ConsoleMode] wri.exe <option> <value> <value> ... <option> <value> ...");
            Console.WriteLine("    option:");
            Console.WriteLine("      -h, --help : value = none");
            Console.WriteLine("      --winapi   : value = WindowsApiクラス内の実行したいメソッド名");
        }
    }
}
