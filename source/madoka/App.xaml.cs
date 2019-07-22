using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using madoka.Common;
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

            AppLogger.Init("madokaLog");
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

            AppLogger.Write($"{c.AppNameWithVersion} Start.");
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            try
            {
                Config.Instance.Save();
                Thread.Sleep(50);
            }
            finally
            {
                AppLogger.Write("madoka End.");
                AppLogger.Flush();
            }
        }

        private void App_DispatcherUnhandledException(
            object sender,
            DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                Config.Instance.Save();
                Thread.Sleep(50);

                if (this.MainWindow != null)
                {
                    MessageBoxHelper.ShowDialogMessageWindow(
                        "madoka - Fatal",
                        "予期しない例外を検知しました。アプリケーションを終了します。",
                        e.Exception);
                }
                else
                {
                    MessageBox.Show(
                        "予期しない例外を検知しました。アプリケーションを終了します。\n\n" +
                        e.Exception,
                        "madoka - Fatal",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            finally
            {
                AppLogger.Fatal(
                    "Unhandled Exception. 予期しない例外が発生しました。",
                    e.Exception);

                AppLogger.Write("madoka Abort.");
                AppLogger.Flush();
            }
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
                catch (Exception ex)
                {
                    AppLogger.Error(
                        "プロセススキャンスレッドで予期しない例外を検知しました。",
                        ex);
                    AppLogger.Flush();

                    Thread.Sleep(TimeSpan.FromSeconds(10));
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
                        !p.Any(x => !x.HasExited))
                    {
                        target.ManagedProcessIDList.Clear();

                        await this.Dispatcher.InvokeAsync(() =>
                        {
                            target.IsRunning = false;
                        });

                        continue;
                    }

                    foreach (var exitedPID in p.Where(x => x.HasExited).Select(x => x.Id))
                    {
                        target.ManagedProcessIDList.Remove(exitedPID);
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
