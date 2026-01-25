using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace wri
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        [STAThread]
        public static void Main()
        {
            App app = new App();
            app.InitializeComponent();
            app.Run();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // コマンドライン引数解析
            CommandLine.Parse(e.Args);
            // 起動モード判定
            if (CommandLine.Option.GuiMode)
            {
                // GUIモード起動
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            }
            else
            {
                // コンソールモード起動
                // コンソールにアタッチ
                global::Utility.WindowsApi.AttachConsole(-1);
                //Console.WriteLine("<wri.exe コンソールモード>");
                Application.Current.Shutdown();
            }
        }
    }

}
