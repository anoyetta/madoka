using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
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
            this.RefreshList();
            this.Config.ManagedWindowList.CollectionChanged += this.ManagedWindowList_CollectionChanged;
        }

        public void Dispose()
        {
            this.Config.ManagedWindowList.CollectionChanged -= this.ManagedWindowList_CollectionChanged;
        }

        private async void ManagedWindowList_CollectionChanged(
            object sender,
            NotifyCollectionChangedEventArgs e)
            => await this.RefreshListAsync();

        public ObservableCollection<ManagedWindowGroupModel> ManagedWindowGroupList
        {
            get;
            private set;
        } = new ObservableCollection<ManagedWindowGroupModel>();

        private async void RefreshList() => await this.RefreshListAsync();

        private async Task RefreshListAsync()
        {
            var source = default(IEnumerable<ManagedWindowModel>);
            lock (this.Config.ManagedWindowList)
            {
                source = this.Config.ManagedWindowList.ToArray();
            }

            if (!source.Any())
            {
                this.ManagedWindowGroupList.Clear();
                return;
            }

            var groupedList = await Task.Run(() =>
            {
                return (
                    from x in source
                    orderby
                    string.IsNullOrEmpty(x.Group) ? 0 : 1,
                    x.Group,
                    x.DisplayName.Value
                    group x by x.Group into g
                    select new ManagedWindowGroupModel()
                    {
                        GroupName = g.First().Group,
                        Children = new ObservableCollection<ManagedWindowModel>(g)
                    }).ToArray();
            });

            this.ManagedWindowGroupList.Clear();
            this.ManagedWindowGroupList.AddRange(groupedList);
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
