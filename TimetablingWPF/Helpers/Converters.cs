using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.VisualStudio.Modeling.Diagrams;

namespace TimetablingWPF
{
    public class PropertyConverter : IValueConverter
    {
        public PropertyConverter(CustomPropertyInfo prop)
        {
            cpi = prop;
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return cpi.Display(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        private readonly CustomPropertyInfo cpi;
    }
    public class XAMLPropertyConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return ((CustomPropertyInfo)values[1]).Display(values[0]);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class MultiplyConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double total = 1;
            for (int i = 0; i < values.Length; i++)
            {
                total *= (double)values[i];
            }
            return total;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class NullToBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ListFormatter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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
        public object ConvertBack(object value, Type targetType, object paramter, CultureInfo culture)
        {
            return null;
        }
    }
    public class ListReportLength : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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
            return $"{length} {((string)parameter).PluralizeFromSingular(length)}";
        }
        public object ConvertBack(object value, Type targetType, object paramter, CultureInfo culture)
        {
            return null;
        }
    }
    public class PeriodsToTable : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return VisualHelpers.GenerateTimetable((IEnumerable<TimetableSlot>)value);
        }
        public object ConvertBack(object value, Type targetType, object paramter, CultureInfo culture)
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
    public class BoxToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            return value[0] == null && string.IsNullOrEmpty((string)value[1]) ? Visibility.Visible : Visibility.Hidden;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class InverseBoolMulti : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return !values.Cast<bool>().Any(x => x);
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class InverseBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class InverseBoolToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Collapsed : Visibility.Visible;
        }
        public object ConvertBack(object value, Type targetType, object paramter, CultureInfo culture)
        {
            return null;
        }
    }
    public class BoolToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object paramter, CultureInfo culture)
        {
            return null;
        }
    }
    public class URIConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.IO.Path.GetFileName((string)value);
        }
        public object ConvertBack(object value, Type targetType, object paramter, CultureInfo culture)
        {
            return null;
        }
    }
    public class DebugConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debugger.Break();
            return value;
        }
        public object ConvertBack(object value, Type targetType, object paramter, CultureInfo culture)
        {
            return null;
        }
    }
    public class MultiplierConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value * double.Parse((string)parameter, CultureInfo.InvariantCulture);
        }
        public object ConvertBack(object value, Type targetType, object paramter, CultureInfo culture)
        {
            return null;
        }
    }

    /// <summary>
    /// Taken from https://thomaslevesque.com/2011/03/21/wpf-how-to-bind-to-data-when-the-datacontext-is-not-inherited/
    /// </summary>
    public class BindingProxy : Freezable
    {
        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }

        public object Data
        {
            get { return GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));
    }

    public class TinterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return VisualHelpers.TintColour((Color)value, double.Parse((string)parameter, CultureInfo.InvariantCulture));
        }
        public object ConvertBack(object value, Type targetType, object paramter, CultureInfo culture)
        {
            return null;
        }
    }

    public class AnimationModifier : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string duration = (string)parameter;
            double modifier = (double)value;
            return new Duration(TimeSpan.FromTicks((long)(TimeSpan.Parse(duration, CultureInfo.InvariantCulture).Ticks * modifier)));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class TimespanModifier : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string duration = (string)parameter;
            double modifier = (double)value;
            return TimeSpan.FromTicks((long)(TimeSpan.Parse(duration, CultureInfo.InvariantCulture).Ticks * modifier));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
