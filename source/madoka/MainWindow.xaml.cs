using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;
using madoka.Common;
using madoka.Models;
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

            this.ViewModel.EnqueueSnackMessageCallback = (message, neverDuplicate) =>
                this.Snackbar.MessageQueue.Enqueue(message, neverDuplicate);

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
                this.WindowState = WindowState.Minimized;

                this.Loaded += async (_, __) =>
                {
                    this.ToHide();
                    this.ShowInTaskbar = true;

                    await Task.Delay(TimeSpan.FromSeconds(0.1));

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

        public MainWindowViewModel ViewModel => this.DataContext as MainWindowViewModel;

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Hide();
                this.NotifyIcon.Visibility = Visibility.Visible;
            }
            else
            {
                this.Show();
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

        public void ToShow()
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.NotifyIcon.Visibility = Visibility.Collapsed;
        }

        public void ToHide()
        {
            this.NotifyIcon.Visibility = Visibility.Visible;
            this.Hide();
        }

        public void ToEnd()
        {
            this.toEnd = true;
            this.Close();
        }

        private void ModelPanel_MouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var model = (sender as Border).DataContext as ManagedWindowModel;
                if (model != null)
                {
                    this.ViewModel.EditModelCommand.Execute(model);
                }
            }
        }
    }
}
