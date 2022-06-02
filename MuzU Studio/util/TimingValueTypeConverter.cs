using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using MuzU.data;

namespace MuzU_Studio.util
{
    internal class TimingValueTypeConverter: IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, string language)
        {
            ValueType valueType = (ValueType)value;
            return valueType._ToString();
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, string language)
        {
            return (value as string).ParseToValueType();
        }
    }
}
