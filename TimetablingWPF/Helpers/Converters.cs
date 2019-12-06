using System;
using System.Collections;
using System.Collections.Generic;
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
}
