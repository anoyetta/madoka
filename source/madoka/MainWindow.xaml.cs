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

            if (Config.Instance.IsMinimizeStartup)
            {
                this.ToHide();
            }
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Visibility = Visibility.Collapsed;
                this.NotifyIcon.Visibility = Visibility.Visible;
            }
            else
            {
                this.Visibility = Visibility.Visible;
                this.NotifyIcon.Visibility = Visibility.Collapsed;
            }
        }

        private volatile bool toEnd;

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (!this.toEnd)
            {
                this.ToHide();
                e.Cancel = true;
                return;
            }

            this.NotifyIcon.Visibility = Visibility.Collapsed;
            this.NotifyIcon.Dispose();
        }

        public void ToggleVisibility()
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.ToShow();
            }
            else
            {
                this.ToHide();
            }
        }

        public void ToShow()
        {
            this.Visibility = Visibility.Visible;
            this.WindowState = WindowState.Normal;
        }

        public void ToHide()
        {
            this.WindowState = WindowState.Minimized;
            this.Visibility = Visibility.Collapsed;
        }

        public void ToEnd()
        {
            this.toEnd = true;
            this.Close();
        }
    }
}
