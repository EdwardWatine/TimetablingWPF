using Humanizer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TimetablingWPF
{
    public static class GenericHelpers
    {
        public static void MoveElementProperties(DependencyObject @in, DependencyObject @out, DependencyProperty[] props)
        {
            foreach (DependencyProperty prop in props)
            {
                object holder = @in.GetValue(prop);
                @in.SetValue(prop, null);
                @out.SetValue(prop, holder);
            }
        }

        public static string Pluralize(this string word, int number)
        {
            return number == 1 ? word : word.Pluralize();
        }

        public static IList<object> InsertAndReturn(IEnumerable enumerable, object toInsert, int index = 0)
        {
            IList<object> list = enumerable.Cast<object>().ToList();
            list.Insert(index, toInsert);
            return new List<object>(list);
        }

        public static string FormatEnumerable(IEnumerable<object> enumerable)
        {
            return string.Join(", ", enumerable);
        }

        public static ErrorContainer GenerateNameError(ErrorManager em, TextBox box, string type)
        {
            ErrorContainer ec = new ErrorContainer(em, (e) => string.IsNullOrWhiteSpace(box.Text),
                (e) => $"{type} has no name.", ErrorType.Error, false);
            box.TextChanged += delegate (object sender, TextChangedEventArgs e) { ec.UpdateError(); };
            return ec;
        }
    }
}
