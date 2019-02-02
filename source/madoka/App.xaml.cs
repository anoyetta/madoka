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
            this.Startup += this.App_Startup;
            this.Exit += this.App_Exit;
            this.DispatcherUnhandledException += this.App_DispatcherUnhandledException;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            // Configをロードする
            var c = Config.Instance;
            c.SetStartup(c.IsStartupWithWindows);

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

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Config.Instance.Save();
        }

        private void DetectProcessLoop()
        {
            while (true)
            {
                Thread.Sleep(TimeSpan.FromSeconds(2));

                try
                {
                    this.DetectProcess();
                }
                catch (ThreadAbortException)
                {
                    return;
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
                    if (p == null)
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

                    if (target.IsLockLocation ||
                        !target.IsLocationApplied)
                    {
                        await target.SetWindowRect();
                    }
                }
                finally
                {
                    Thread.Yield();
                }
            }
        }
    }
}
