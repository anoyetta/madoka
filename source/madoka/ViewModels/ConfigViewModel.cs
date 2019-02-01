using Prism.Mvvm;

namespace madoka.ViewModels
{
    public class ConfigViewModel : BindableBase
    {
        public Config Config => Config.Instance;
    }
}
