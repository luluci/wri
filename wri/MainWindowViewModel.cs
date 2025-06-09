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
    using Microsoft.Web.WebView2.WinForms;
    using System.Windows;
    using System.Windows.Controls;
    using Utility;

    class MainWindowViewModel : BindableBase, IDisposable
    {
        // 
        MainWindow window;

        //
        public ReactivePropertySlim<string> WindowTitle { get; set; }
        public ReactivePropertySlim<GridLength> HeaderHeight { get; set; }
        public ReactivePropertySlim<GridLength> FooterHeight { get; set; }
        public ReactivePropertySlim<GridLength> HeaderSplitterHeight { get; set; }
        public ReactivePropertySlim<GridLength> FooterSplitterHeight { get; set; }
        public ReactivePropertySlim<Visibility> HeaderVisibility { get; set; }
        public ReactivePropertySlim<Visibility> FooterVisibility { get; set; }
        // WebView2
        public ReactivePropertySlim<Uri> SourcePath { get; set; }
        // WebView2 WebView2CompositionControl
        public WebView2CompositionControl WebView2;
        public Interface.EntryPoint EntryPoint { get; set; }

        //
        Uri uriBlank = new Uri("about:blank");

        public MainWindowViewModel(MainWindow window)
        {
            // InitializeComponent()の前にインスタンス化する
            this.window = window;

            //
            HeaderSplitterHeight = new ReactivePropertySlim<GridLength>(new GridLength(0));
            HeaderSplitterHeight.AddTo(Disposables);
            FooterSplitterHeight = new ReactivePropertySlim<GridLength>(new GridLength(0));
            FooterSplitterHeight.AddTo(Disposables);
            HeaderHeight = new ReactivePropertySlim<GridLength>(new GridLength(0));
            HeaderHeight.AddTo(Disposables);
            FooterHeight = new ReactivePropertySlim<GridLength>(new GridLength(0));
            FooterHeight.AddTo(Disposables);
            HeaderVisibility = new ReactivePropertySlim<Visibility>(Visibility.Collapsed);
            HeaderVisibility.AddTo(Disposables);
            FooterVisibility = new ReactivePropertySlim<Visibility>(Visibility.Collapsed);
            FooterVisibility.AddTo(Disposables);

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
            Uri uri;
            if (object.ReferenceEquals(CommandLine.Option.LaunchFile, string.Empty))
            {
                uri = new Uri($@"{rootPath}\index.html");
            }
            else
            {
                uri = new Uri(CommandLine.Option.LaunchFile);
            }
            //uri = new Uri("https://www.google.co.jp");

            SourcePath = new ReactivePropertySlim<Uri>(uri);
            SourcePath.AddTo(Disposables);
        }

        public async Task InitAsync(MainWindow window)
        {
            window.Show();

            // test
            HeaderVisibility.Value = Visibility.Visible;
            HeaderHeight.Value = new GridLength(50);
            HeaderSplitterHeight.Value = new GridLength(10);
            FooterVisibility.Value = Visibility.Visible;
            FooterHeight.Value = new GridLength(50);
            FooterSplitterHeight.Value = new GridLength(10);

            // WebView2初期化処理
            WebView2 = window.WebView2;
            // 初期化完了ハンドラ登録
            WebView2.CoreWebView2InitializationCompleted += webView2CoreWebView2InitializationCompleted;
            WebView2.NavigationStarting += webView_NavigationStarting;
            WebView2.NavigationCompleted += webView_NavigationCompleted;
            // JavaScript側からの呼び出し
            WebView2.WebMessageReceived += webView_WebMessageReceived;

            //var opt = WebView2.CreationProperties;

            // 適切なタイミングで初期化する
            Interface.GlobalData.WebView2 = WebView2;
            EntryPoint = new Interface.EntryPoint();

            //WebView2.AllowExternalDrop = true;
            //WebView2.AllowDrop = true;
            //window.Base.DragEnter += (object sender, System.Windows.DragEventArgs e) =>
            //{
            //    if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            //    {
            //        e.Effects = System.Windows.DragDropEffects.Copy;
            //    }
            //    else
            //    {
            //        e.Effects = System.Windows.DragDropEffects.None;
            //    }
            //};
            window.Base.DragOver += (object sender, System.Windows.DragEventArgs e) =>
            {
                if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
                {
                    e.Effects = System.Windows.DragDropEffects.Copy;
                }
                else
                {
                    e.Effects = System.Windows.DragDropEffects.None;
                }
            };
            window.Base.Drop += (object sender, System.Windows.DragEventArgs e) =>
            {
                if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
                {
                    if (e.Data.GetData(System.Windows.DataFormats.FileDrop) is string[] files)
                    {
                        var file = files[0];
                        var ext = System.IO.Path.GetExtension(file).ToLower();
                        if (ext == ".html")
                        {
                            var check = CheckPreventClose();
                            if (!check)
                            {
                                var uri = new Uri(file);
                                SourcePath.Value = uri;
                            }
                        }
                    }
                }
                else
                {
                    e.Effects = System.Windows.DragDropEffects.None;
                }
            };

            // WebView2コア初期化
            await WebView2.EnsureCoreWebView2Async();

            //WebView2.CoreWebView2.PermissionRequested += CoreWebView2_PermissionRequested;
        }

        private void webView2CoreWebView2InitializationCompleted(object sender, EventArgs e)
        {
            // WebView2起動時の初期化完了後に1回だけコールされる
        }

        private void webView_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            var uri = new Uri(e.Uri);
            if (uri.Scheme != "file")
            {
                SourcePath.Value = uriBlank;
                e.Cancel = true;
                //
                // HTML文字列を表示
                //string htmlContent = @"
                //    <html>
                //    <head>
                //        <title>Warning</title>
                //    </head>
                //    <body>
                //        <h1>ローカルファイルのみ開けます</h1>
                //    </body>
                //    </html>
                //";

                //WebView2.CoreWebView2.NavigateToString(htmlContent);
            }
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
                //
                if (!object.ReferenceEquals(WebView2.Source, uriBlank))
                {
                    await RunScriptLoaded("csLoaded()");
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "初期化エラー");
                //System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private void webView_WebMessageReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            // WebView2からのメッセージ受信イベント
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

        //private void CoreWebView2_PermissionRequested(object sender, CoreWebView2PermissionRequestedEventArgs e)
        //{
        //    // Example: Automatically grant geolocation permission
        //    if (e.PermissionKind == CoreWebView2PermissionKind.FileReadWrite)
        //    {
        //        e.State = CoreWebView2PermissionState.Allow;
        //    }
        //    else
        //    {
        //        // Deny other permissions by default
        //        e.State = CoreWebView2PermissionState.Deny;
        //    }
        //}

        private bool CheckPreventClose()
        {
            // WebView2終了防止チェック
            if (EntryPoint.preventClose)
            {
                var result = System.Windows.MessageBox.Show(EntryPoint.preventCloseMsg + "\r\n終了してよろしいですか？", "wri", System.Windows.MessageBoxButton.YesNo);
                if (result == System.Windows.MessageBoxResult.No)
                {
                    // 終了を防止する
                    return true;
                }
            }
            return false;
        }

        public bool OnClosing()
        {
            // WebView2の終了を防止する
            // true:アプリ終了を防ぐ
            // false:アプリ終了を許可する
            return CheckPreventClose();
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
