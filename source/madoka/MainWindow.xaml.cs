using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;
using madoka.Common;
using madoka.ViewModels;
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

            this.Closed += (_, __) =>
            {
                var vm = this.DataContext as MainWindowViewModel;
                if (vm != null)
                {
                    vm.Dispose();
                }
            };

            if (Config.Instance.IsMinimizeStartup)
            {
                this.ShowInTaskbar = false;

                this.Loaded += async (_, __) =>
                {
                    this.ToHide();

                    await Task.Delay(TimeSpan.FromSeconds(0.1));
                    this.ShowInTaskbar = true;

                    await WPFHelper.Dispatcher.InvokeAsync(() =>
                    {
                        this.NotifyIcon.ShowBalloonTip(
                            Config.Instance.AppNameWithVersion,
                            "Started",
                            BalloonIcon.Info);
                    },
                    DispatcherPriority.ApplicationIdle);
                };
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
