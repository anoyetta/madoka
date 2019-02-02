using System;
using System.Collections.Specialized;
using madoka.Models;
using madoka.Views;
using Prism.Commands;
using Prism.Mvvm;

namespace madoka.ViewModels
{
    public class MainWindowViewModel :
        BindableBase,
        IDisposable
    {
        public Config Config => Config.Instance;

        public MainWindowViewModel()
        {
            this.Config.ManagedWindowList.CollectionChanged += this.ManagedWindowList_CollectionChanged;
        }

        public void Dispose()
        {
            this.Config.ManagedWindowList.CollectionChanged -= this.ManagedWindowList_CollectionChanged;
        }

        private void ManagedWindowList_CollectionChanged(
            object sender,
            NotifyCollectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        #region Commands

        private DelegateCommand addCommand;

        public DelegateCommand AddCommand =>
            this.addCommand ?? (this.addCommand = new DelegateCommand(this.ExecuteAddCommand));

        private void ExecuteAddCommand()
        {
            var view = new AppSettingsView()
            {
                Owner = MainWindow.Instance,
            };

            (view.DataContext as AppSettingsViewModel).Model = new ManagedWindowModel();

            view.Show();
        }

        private DelegateCommand showConfigCommand;

        public DelegateCommand ShowConfigCommand =>
            this.showConfigCommand ?? (this.showConfigCommand = new DelegateCommand(this.ExecuteShowConfigCommand));

        private void ExecuteShowConfigCommand()
        {
            var view = new ConfigView()
            {
                Owner = MainWindow.Instance,
            };

            view.Show();
        }

        #endregion Commands

        #region ContextMenu Commands

        private DelegateCommand showCommand;

        public DelegateCommand ShowCommand =>
            this.showCommand ?? (this.showCommand = new DelegateCommand(this.ExecuteShowCommand));

        private void ExecuteShowCommand()
        {
            MainWindow.Instance.ToShow();
        }

        private DelegateCommand endCommand;

        public DelegateCommand EndCommand =>
            this.endCommand ?? (this.endCommand = new DelegateCommand(this.ExecuteEndCommand));

        private void ExecuteEndCommand()
        {
            MainWindow.Instance.ToEnd();
        }

        #endregion ContextMenu Commands
    }
}
