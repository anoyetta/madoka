using madoka.ViewModels;
using MahApps.Metro.Controls;

namespace madoka.Views
{
    /// <summary>
    /// ConfigView.xaml の相互作用ロジック
    /// </summary>
    public partial class ConfigView : MetroWindow
    {
        public ConfigView()
        {
            this.InitializeComponent();

            this.Closed += (_, __) =>
            {
                var config = (this.DataContext as ConfigViewModel)?.Config;
                config.Save();
            };
        }
    }
}
