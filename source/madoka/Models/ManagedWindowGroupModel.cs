using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace madoka.Models
{
    public class ManagedWindowGroupModel : BindableBase
    {
        private string groupName;

        public string GroupName
        {
            get => this.groupName;
            set
            {
                if (this.SetProperty(ref this.groupName, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsExistsGroupName));
                }
            }
        }

        public bool IsExistsGroupName => !string.IsNullOrEmpty(this.GroupName);

        private ObservableCollection<ManagedWindowModel> children = new ObservableCollection<ManagedWindowModel>();

        public ObservableCollection<ManagedWindowModel> Children
        {
            get => this.children;
            set
            {
                this.children.Clear();

                foreach (var item in value)
                {
                    item.Parrent = this;
                    this.children.Add(item);
                }
            }
        }

        public async Task<int> RunAppsAsync() => await Task.Run(async () =>
        {
            var count = 0;

            var targets = this.children.Where(x => x.IsEnabled).ToArray();
            foreach (var app in targets)
            {
                await Task.Delay(TimeSpan.FromSeconds(0.1));
                if (await app.Run())
                {
                    count++;
                }
            }

            return count;
        });
    }
}
