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
            if (e.Args.Count() > 0)
            {
                // コマンドライン引数が存在するとき
                // コンソールアプリモードで処理
                foreach (string arg in e.Args)
                {
                    MessageBox.Show(arg);
                }

            }
            else
            {
                // コマンドライン引数が存在しないとき
                // GUI起動
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            }
        }
    }

}
