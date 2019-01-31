using System;
using System.ComponentModel;
using System.Windows;
using MahApps.Metro.Controls;

namespace madoka
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public static MainWindow Instance { get; private set; }

        public MainWindow()
        {
            Instance = this;

            this.InitializeComponent();

            this.StateChanged += this.MainWindow_StateChanged;
            this.Closing += this.MainWindow_Closing;
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.Visibility = Visibility.Visible;
            }
        }

        private volatile bool toEnd;

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (!this.toEnd)
            {
                this.WindowState = WindowState.Minimized;
                e.Cancel = true;
                return;
            }

            this.NotifyIcon.Visibility = Visibility.Collapsed;
            this.NotifyIcon.Dispose();
        }

        public void ToShow()
        {
            this.WindowState = WindowState.Normal;
        }

        public void ToHide()
        {
            this.WindowState = WindowState.Minimized;
        }

        public void ToEnd()
        {
            this.toEnd = true;
            this.Close();
        }
    }
}
