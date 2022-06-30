using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using MuzU.data;
using Windows.UI.Xaml;

namespace MuzU_Studio.util
{
    internal class NotVisibilityConverter: IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, string language)
        {
            bool isCollapsed = (bool)value;
            return (isCollapsed)? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, string language)
        {
            Visibility visibility = (Visibility)value;
            return visibility==Visibility.Collapsed;
        }
    }
}
