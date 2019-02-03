using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using madoka.Common;
using madoka.Models;
using MahApps.Metro.Controls.Dialogs;
using Prism.Commands;
using Prism.Mvvm;

namespace madoka.ViewModels
{
    public class AppSettingsViewModel : BindableBase
    {
        public Config Config => Config.Instance;

        private ManagedWindowModel model = WPFHelper.IsDesignMode ? new ManagedWindowModel() : null;

        public ManagedWindowModel Model
        {
            get => this.model;
            set => this.SetProperty(ref this.model, value);
        }

        public Action CloseAction { get; set; }

        public WPFHelper.ShowMessageAsyncDelegate ShowMessageAsyncCallback { get; set; }

        public WPFHelper.EnqueueSnackMessageDelegate EnqueueSnackMessageCallback { get; set; }

        public IEnumerable<EnumContainer<DPIAwares>> DPIAwaresValues =>
            EnumConverter.ToEnumerableContainer<DPIAwares>()
            .Where(x => x.Value != DPIAwares.Unknown);

        private ICommand checkCommand;

        public ICommand CheckCommand =>
            this.checkCommand ?? (this.checkCommand = new DelegateCommand(async () =>
                {
                    if (!string.IsNullOrEmpty(this.model.Exe))
                    {
                        await Task.WhenAll(
                            this.model.SetRegistry(),
                            this.model.SetWindowRect(true));

                        lock (this.Config.ManagedWindowList)
                        {
                            if (!this.Config.ManagedWindowList.Any(x => x.ID == this.model.ID))
                            {
                                this.Config.ManagedWindowList.Add(this.model);
                            }
                        }

                        this.CloseAction?.Invoke();
                    }
                }));

        private ICommand deleteCommand;

        public ICommand DeleteCommand =>
            this.deleteCommand ?? (this.deleteCommand = new DelegateCommand(async () =>
            {
                var result = await this.ShowMessageAsyncCallback?.Invoke(
                    "確認",
                    $"{this.model.DisplayName} の設定を削除しますか？",
                    MessageDialogStyle.AffirmativeAndNegative);

                if (result != MessageDialogResult.Affirmative)
                {
                    return;
                }

                lock (this.Config.ManagedWindowList)
                {
                    if (this.Config.ManagedWindowList.Any(x => x.ID == this.model.ID))
                    {
                        this.Config.ManagedWindowList.Remove(this.model);
                        this.CloseAction?.Invoke();
                    }
                }
            }));

        private ICommand runCommand;

        public ICommand RunCommand =>
            this.runCommand ?? (this.runCommand = new DelegateCommand(async () =>
            {
                var r = await this.model.Run();
                if (r)
                {
                    this.EnqueueSnackMessageCallback?.Invoke(
                        $"{this.model.DisplayName} を起動しました。");
                }
            }));

        private ICommand getProcessInfoCommand;

        public ICommand GetProcessInfoCommand =>
            this.getProcessInfoCommand ?? (this.getProcessInfoCommand = new DelegateCommand(async () =>
                await Task.WhenAll(
                    this.model.GetWindowInfo(),
                    this.model.GetProcessDPIAwareness())));

        private ICommand applyLayoutCommand;

        public ICommand ApplyLayoutCommand =>
            this.applyLayoutCommand ?? (this.applyLayoutCommand = new DelegateCommand(async () =>
            {
                var r = await this.model.SetWindowRect(true);
                if (r)
                {
                    this.EnqueueSnackMessageCallback?.Invoke(
                        $"{this.model.DisplayName} にレイアウトを適用しました。");
                }
            }));

        private ICommand applyScalingCommand;

        public ICommand ApplyScalingCommand =>
            this.applyScalingCommand ?? (this.applyScalingCommand = new DelegateCommand(async () =>
            {
                var r = await this.model.SetRegistry();
                if (r)
                {
                    this.EnqueueSnackMessageCallback?.Invoke(
                        $"互換性及びDPIスケーリング設定をレジストリに登録しました。");
                }
            }));
    }
}
