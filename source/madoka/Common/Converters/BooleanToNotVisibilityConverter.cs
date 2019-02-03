using System.Windows;

namespace madoka.Common
{
    public class BooleanToNotVisibilityConverter :
        BooleanConverter<Visibility>
    {
        public BooleanToNotVisibilityConverter() :
            base(Visibility.Collapsed, Visibility.Visible)
        {
        }
    }
}
