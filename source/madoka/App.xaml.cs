using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using madoka.Models;

namespace madoka
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private Thread detectProcessThread;

        public App()
        {
            this.DispatcherUnhandledException += this.App_DispatcherUnhandledException;
            this.Startup += this.App_Startup;
            this.Exit += this.App_Exit;

            this.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            // Configをロードする
            var c = Config.Instance;
            c.SetStartup(c.IsStartupWithWindows);

            if (!c.ManagedWindowList.Any())
            {
                var model = new ManagedWindowModel()
                {
                    Exe = @"C:\Windows\notepad.exe",
                    IsSetLocation = true,
                    X = 20,
                    Y = 20,
                };

                c.ManagedWindowList.Add(model);
            }

            // 監視スレッドを生成する
            this.detectProcessThread = new Thread(new ThreadStart(this.DetectProcessLoop))
            {
                IsBackground = true,
                Priority = ThreadPriority.Lowest,
            };

            this.detectProcessThread.Start();
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            Config.Instance.Save();
        }

        private void App_DispatcherUnhandledException(
            object sender,
            DispatcherUnhandledExceptionEventArgs e)
        {
            Config.Instance.Save();

            var message = string.Empty;
            message += "予期しない例外が発生しました。アプリケーションを終了します。\n\n";
            message += e.Exception.ToString();

            MessageBox.Show(
                message,
                "madoka - Faital",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        private volatile bool semaphore = false;

        private void DetectProcessLoop()
        {
            Thread.Sleep(TimeSpan.FromSeconds(Config.Instance.ProcessScaningInterval));

            while (true)
            {
                Thread.Sleep(TimeSpan.FromSeconds(Config.Instance.ProcessScaningInterval));

                if (this.semaphore)
                {
                    continue;
                }

                try
                {
                    this.semaphore = true;
                    this.DetectProcess();
                }
                catch (ThreadAbortException)
                {
                    return;
                }
                finally
                {
                    this.semaphore = false;
                }
            }
        }

        private async void DetectProcess()
        {
            var list = default(IEnumerable<ManagedWindowModel>);
            lock (Config.Instance.ManagedWindowList)
            {
                list = Config.Instance.ManagedWindowList.ToArray();
            }

            var targets = list
                .Where(x => x.IsEnabled)
                .ToArray();

            foreach (var target in targets)
            {
                try
                {
                    if (string.IsNullOrEmpty(target.Exe) ||
                        !File.Exists(target.Exe))
                    {
                        continue;
                    }

                    var p = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(target.Exe));
                    if (p == null ||
                        p.Length <= 0)
                    {
                        await this.Dispatcher.InvokeAsync(() =>
                        {
                            target.IsRunning = false;
                        });

                        continue;
                    }

                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        target.IsRunning = true;
                    });

                    await target.GetProcessDPIAwareness();
                    await target.SetWindowRect(target.IsLockLocation);
                }
                finally
                {
                    Thread.Yield();
                }
            }
        }
    }
}
