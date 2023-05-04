﻿using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace wri
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        MainWindowViewModel vm;

        public MainWindow()
        {
            vm = new MainWindowViewModel(this);
            this.DataContext = vm;

            InitializeComponent();

        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await vm.InitAsync(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "初期化エラー");
            }
        }

        private void WebView2_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            int i;
            i = 0;
        }
    }
}
