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
        public List<string> LaunchParameter;

        public CommandLineOption()
        {
            GuiMode = true;
            LaunchFile = null;
            LaunchParameter = new List<string>();
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
            // GUIモード起動チェック
            // デフォルトでGUIモード
            Option.GuiMode = true;
            int i = 0;
            for (; i<args.Length; i++)
            {
                var arg = args[i];
                // オプション指定が出現したら終了
                if (arg.StartsWith("-"))
                {
                    break;
                }
                // 引数解析
                // 先頭のみ起動時htmlを指定可能
                if (i == 0)
                {
                    // ファイル存在チェック
                    if (System.IO.File.Exists(arg))
                    {
                        // 引数情報を取得
                        var ext = System.IO.Path.GetExtension(arg);
                        switch (ext)
                        {
                            case ".html":
                                // 存在するhtmlのみ指定可能
                                Option.LaunchFile = arg;
                                break;

                            default:
                                // それ以外はパラメータとして扱う
                                Option.LaunchParameter.Add(arg);
                                break;
                        }
                    }
                    else
                    {
                        Option.LaunchParameter.Add(arg);
                    }
                }
                else
                {
                    // 2つ目以降の引数はパラメータとして扱う
                    Option.LaunchParameter.Add(arg);
                }
            }

            // コマンドライン引数をチェック
            for (; i < args.Length; i++)
            {
                var arg = args[i];
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
