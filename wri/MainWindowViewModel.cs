using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reactive.Disposables;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace wri
{
    using Utility;

    class MainWindowViewModel : BindableBase, IDisposable
    {
        // 
        MainWindow window;
        //
        public ReactivePropertySlim<string> WindowTitle { get; set; }
        public ReactivePropertySlim<Uri> ScriptPath { get; set; }
        // WebView2
        public WebView2 WebView2;
        public Interface.EntryPoint EntryPoint { get; set; }

        public MainWindowViewModel(MainWindow window)
        {
            // InitializeComponent()の前にインスタンス化する
            this.window = window;

            // WebView2インスタンスの初期化前に実施する
            // dllをexeファイル内に取り込むのと相性が悪い。
            // WebView2Loader.dllの場所を明示する。
            string rootPath = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            CoreWebView2Environment.SetLoaderDllFolderPath(rootPath);

            WindowTitle = new ReactivePropertySlim<string>("wri");
            WindowTitle.AddTo(Disposables);

            // index.htmlへのパスを作成
            //string rootPath = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            //string SettingPath = rootPath + @"\Script";
            var uri = new Uri($@"{rootPath}\apps\index.html");

            ScriptPath = new ReactivePropertySlim<Uri>(uri);
            ScriptPath.AddTo(Disposables);
        }

        public async Task InitAsync(MainWindow window)
        {
            window.Show();
            // WebView2初期化処理
            WebView2 = window.WebView2;
            // 初期化完了ハンドラ登録
            WebView2.CoreWebView2InitializationCompleted += webView2CoreWebView2InitializationCompleted;
            WebView2.NavigationCompleted += webView_NavigationCompleted;
            // JavaScript側からの呼び出し
            WebView2.WebMessageReceived += webView_WebMessageReceived;

            // 適切なタイミングで初期化する
            EntryPoint = new Interface.EntryPoint();

            // WebView2コア初期化
            await WebView2.EnsureCoreWebView2Async();
        }

        private void webView2CoreWebView2InitializationCompleted(object sender, EventArgs e)
        {
            // WebView2起動時の初期化完了後に1回だけコールされる
        }

        private async void webView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            try
            {
                //webView.CoreWebView2.PostWebMessageAsString("C#からのデータ送信");
                WindowTitle.Value = "wri - " + WebView2.Source.ToString();
                EntryPoint.Source = WebView2.Source;
                EntryPoint.SourcePath = WebView2.Source.LocalPath;

                // 表示完了時にコールされる。F5による更新時にもコールされる。
                // このときObjectの登録等も初期化されるため、毎回登録する。
                // API登録
                // 登録するインスタンスのclassはアクセスレベルをpublicにしないとエラー
                // wriアプリのAPIなのでwriとしておく
                WebView2.CoreWebView2.AddHostObjectToScript("wri", EntryPoint);

                // WebView2/index.htmlのJavaScript初期化
                await RunScriptLoaded("const wri = chrome.webview.hostObjects.sync.wri; true");
                await RunScriptLoaded("const wri_async = chrome.webview.hostObjects.wri; true");
                await RunScriptLoaded("csLoaded()");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "初期化エラー");
                //System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private void webView_WebMessageReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            var s = e.TryGetWebMessageAsString();
            //MessageBox.Show(s);
        }

        public async Task RunScriptLoaded(string handler)
        {
            int limit = 0;
            while (await WebView2.ExecuteScriptAsync(handler) != "true")
            {
                limit++;
                if (limit > 50)
                {
                    throw new Exception("WebView2の初期化に失敗したようです。");
                }
                await Task.Delay(100);
            }
        }

        #region IDisposable Support
        private CompositeDisposable Disposables { get; } = new CompositeDisposable();
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)。
                    this.Disposables.Dispose();
                }

                // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        // ~MainWindowViewModel() {
        //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
        //   Dispose(false);
        // }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        void IDisposable.Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
