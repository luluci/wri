using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace wri
{
    public class CommandLineOption
    {
        // 起動モード
        public bool GuiMode;

        // GUIモード
        public string LaunchFile;

        public CommandLineOption()
        {
            GuiMode = true;
            LaunchFile = string.Empty;
        }
    }

    static public class CommandLine
    {
        static public CommandLineOption Option { get; set; } = new CommandLineOption();

        // コンソールモード
        static Interface.EntryPoint ep = new Interface.EntryPoint();
        static Type WinApiT = typeof(Interface.WindowsApi);
        // 解析状態
        enum OptionState
        {
            None,
            WinApi,
        }
        static OptionState optionState = OptionState.None;

        static public void Parse(string[] args)
        {
            if (args.Length > 0)
            {
                var ext = System.IO.Path.GetExtension(args[0]);
                switch (ext)
                {
                    case ".html":
                        Option.LaunchFile = args[0];
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
                        Option.GuiMode = false;
                        Help();
                        return;

                    case "--winapi":
                        Option.GuiMode = false;
                        optionState = OptionState.WinApi;
                        break;

                    default:
                        ParseValue(arg);
                        break;
                }
            }
        }

        static private void ParseValue(string value)
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

        static private void ParseValueWinApi(string value)
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

        static public void Help()
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
