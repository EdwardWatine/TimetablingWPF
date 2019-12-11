using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TimetablingWPF
{
    public class ListFormatter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return "N/A";
            }
            if (((IList)value).Count == 0)
            {
                return "None";
            }
            IEnumerable<object> enumerable = ((IEnumerable)value).Cast<object>();
            return GenericHelpers.FormatEnumerable(enumerable);
        }
        public object ConvertBack(object value, Type targetType, object paramter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
    public class ListReportLength : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return "N/A";
            }
            int length = ((IList)value).Count;
            if (length == 0)
            {
                return "None";
            }
            return $"{length} {((string)parameter).Pluralize(length)}";
        }
        public object ConvertBack(object value, Type targetType, object paramter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
    public class PeriodsToTable : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return VisualHelpers.GenerateTimetable((IEnumerable<TimetableSlot>)value);
        }
        public object ConvertBack(object value, Type targetType, object paramter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
    public class ItemRepr : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty((string)value[1]) ? value[0].ToString() : value[0].GetType().GetProperty((string)value[1]).GetValue(value[0]).ToString();

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
