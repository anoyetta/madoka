using Prism.Mvvm;

namespace madoka.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        public Config Config => Config.Instance;
    }
}
