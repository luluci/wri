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
            // 特定のコマンドでコンソールモードで動作する
            CommandLine.Parse(e.Args);
            //
            if (CommandLine.Option.GuiMode)
            {
                // コマンドライン引数が存在しないとき
                // GUI起動
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            }
            else
            {
                // コマンドライン引数が存在するとき
                // コンソールアプリモードで処理
                // コンソールにアタッチ
                global::Utility.WindowsApi.AttachConsole(-1);
                //Console.WriteLine("<wri.exe コンソールモード>");
                Application.Current.Shutdown();
            }
        }
    }

}
