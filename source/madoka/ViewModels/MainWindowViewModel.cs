using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using madoka.Common;
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

        public WPFHelper.EnqueueSnackMessageDelegate EnqueueSnackMessageCallback { get; set; }

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

        public SuspendableObservableCollection<ManagedWindowGroupModel> ManagedWindowGroupList
        {
            get;
            private set;
        } = new SuspendableObservableCollection<ManagedWindowGroupModel>();

        private async void RefreshList() => await this.RefreshListAsync();

        private async Task RefreshListAsync()
        {
            if (WPFHelper.IsDesignMode)
            {
                this.CreateDesigntimeList();
                return;
            }

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

            var groupedList = await Task.Run(() => (
                from x in source
                orderby
                string.IsNullOrEmpty(x.Group) ? 0 : 1,
                x.Group,
                x.DisplayName
                group x by x.Group into g
                select new ManagedWindowGroupModel()
                {
                    GroupName = g.First().Group,
                    Children = new ObservableCollection<ManagedWindowModel>(g)
                }).ToArray());

            this.ManagedWindowGroupList.AddRange(groupedList, true);
        }

        private void CreateDesigntimeList()
        {
            var list = new List<ManagedWindowGroupModel>()
            {
                new ManagedWindowGroupModel()
                {
                    GroupName = string.Empty,
                    Children = new ObservableCollection<ManagedWindowModel>()
                    {
                        new ManagedWindowModel()
                        {
                            Exe = "notepad.exe"
                        },
                        new ManagedWindowModel()
                        {
                            Exe = "twtter.exe"
                        },
                    }
                },
                new ManagedWindowGroupModel()
                {
                    GroupName = "ビジネス",
                    Children = new ObservableCollection<ManagedWindowModel>()
                    {
                        new ManagedWindowModel()
                        {
                            Exe = "word.exe"
                        },
                        new ManagedWindowModel()
                        {
                            Exe = "excel.exe"
                        },
                    }
                },
                new ManagedWindowGroupModel()
                {
                    GroupName = "ゲーム",
                    Children = new ObservableCollection<ManagedWindowModel>()
                    {
                        new ManagedWindowModel()
                        {
                            Exe = "ffxiv_dx11.exe"
                        },
                        new ManagedWindowModel()
                        {
                            Exe = "ffxiv.exe"
                        },
                        new ManagedWindowModel()
                        {
                            Exe = "mhw.exe"
                        },
                    }
                },
            };

            this.ManagedWindowGroupList.AddRange(list);
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
            (view.DataContext as AppSettingsViewModel).RefreshManagedWindowsListCallback = () => this.RefreshList();

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

        private DelegateCommand<ManagedWindowModel> editModelCommand;

        public DelegateCommand<ManagedWindowModel> EditModelCommand =>
            this.editModelCommand ?? (this.editModelCommand = new DelegateCommand<ManagedWindowModel>((model) =>
            {
                if (model == null)
                {
                    return;
                }

                var view = new AppSettingsView()
                {
                    Owner = MainWindow.Instance,
                };

                (view.DataContext as AppSettingsViewModel).Model = model;
                (view.DataContext as AppSettingsViewModel).RefreshManagedWindowsListCallback = () => this.RefreshList();

                view.Show();
            }));

        private DelegateCommand<ManagedWindowModel> applyLayoutCommand;

        public DelegateCommand<ManagedWindowModel> ApplyLayoutCommand =>
            this.applyLayoutCommand ?? (this.applyLayoutCommand = new DelegateCommand<ManagedWindowModel>(async (model) =>
            {
                var r = await model?.SetWindowRect(true);
                if (r)
                {
                    this.EnqueueSnackMessageCallback?.Invoke(
                        $"{model.DisplayName} にレイアウトを適用しました。");
                }
            }));

        private DelegateCommand<ManagedWindowModel> runAppCommand;

        public DelegateCommand<ManagedWindowModel> RunAppCommand =>
            this.runAppCommand ?? (this.runAppCommand = new DelegateCommand<ManagedWindowModel>(async (model) =>
            {
                var r = await model?.Run();
                if (r)
                {
                    this.EnqueueSnackMessageCallback?.Invoke(
                        $"{model.DisplayName} を起動しました。");
                }
            }));

        private DelegateCommand<ManagedWindowGroupModel> runGroupAppsCommand;

        public DelegateCommand<ManagedWindowGroupModel> RunGroupAppsCommand =>
            this.runGroupAppsCommand ?? (this.runGroupAppsCommand = new DelegateCommand<ManagedWindowGroupModel>(async (groupModel) =>
            {
                var count = await groupModel?.RunAppsAsync();
                if (count > 0)
                {
                    this.EnqueueSnackMessageCallback?.Invoke(
                        $"{count:N0} 件のアプリケーションを起動しました。");
                }
            }));

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
