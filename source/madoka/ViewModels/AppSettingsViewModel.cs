using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using madoka.Common;
using madoka.Models;
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
                            this.model.SetWindowRect());

                        lock (this.Config.ManagedWindowList)
                        {
                            if (!this.Config.ManagedWindowList.Any(x =>
                                string.Equals(x.Exe, this.model.Exe, StringComparison.OrdinalIgnoreCase)))
                            {
                                this.Config.ManagedWindowList.Add(this.model);
                            }
                        }
                    }
                }));

        private ICommand deleteCommand;

        public ICommand DeleteCommand =>
            this.deleteCommand ?? (this.deleteCommand = new DelegateCommand(() =>
            {
                lock (this.Config.ManagedWindowList)
                {
                    if (!this.Config.ManagedWindowList.Contains(this.model))
                    {
                        this.Config.ManagedWindowList.Remove(this.model);
                        this.CloseAction?.Invoke();
                    }
                }
            }));

        private ICommand runCommand;

        public ICommand RunCommand =>
            this.runCommand ?? (this.runCommand = new DelegateCommand(async () => await this.model.Run()));

        private ICommand getProcessInfoCommand;

        public ICommand GetProcessInfoCommand =>
            this.getProcessInfoCommand ?? (this.getProcessInfoCommand = new DelegateCommand(async () =>
                await Task.WhenAll(
                    this.model.GetWindowInfo(),
                    this.model.GetProcessDPIAwareness())));

        private ICommand applyLayoutCommand;

        public ICommand ApplyLayoutCommand =>
            this.applyLayoutCommand ?? (this.applyLayoutCommand = new DelegateCommand(async () => await this.model.SetWindowRect()));

        private ICommand applyScalingCommand;

        public ICommand ApplyScalingCommand =>
            this.applyScalingCommand ?? (this.applyScalingCommand = new DelegateCommand(async () => await this.model.SetRegistry()));
    }
}
