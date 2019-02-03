using System.Windows;

namespace madoka.Common
{
    public class BooleanToHiddenConverter :
        BooleanConverter<Visibility>
    {
        public BooleanToHiddenConverter() :
            base(Visibility.Visible, Visibility.Hidden)
        {
        }
    }
}
