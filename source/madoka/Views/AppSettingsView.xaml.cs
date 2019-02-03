using System.Windows.Input;
using madoka.ViewModels;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace madoka.Views
{
    /// <summary>
    /// AppSettingsView.xaml の相互作用ロジック
    /// </summary>
    public partial class AppSettingsView : MetroWindow
    {
        public AppSettingsView()
        {
            this.InitializeComponent();

            this.KeyUp += (_, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    this.Close();
                }
            };

            this.Closed += (_, __) => Config.Instance.Save();

            this.Loaded += (_, __) =>
            {
                var vm = this.DataContext as AppSettingsViewModel;
                if (vm != null)
                {
                    vm.CloseAction = this.Close;
                    vm.ShowMessageAsyncCallback = (a, b, c, d) => this.ShowMessageAsync(a, b, c, d);
                    vm.EnqueueSnackMessageCallback = (message, neverDuplicate) =>
                        this.Snackbar.MessageQueue.Enqueue(message, neverDuplicate);
                }
            };
        }
    }
}
