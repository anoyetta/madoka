using System.Collections.ObjectModel;
using Prism.Mvvm;

namespace madoka.Models
{
    public class ManagedWindowGroupModel : BindableBase
    {
        private string groupName;

        public string GroupName
        {
            get => this.groupName;
            set => this.SetProperty(ref this.groupName, value);
        }

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
    }
}
