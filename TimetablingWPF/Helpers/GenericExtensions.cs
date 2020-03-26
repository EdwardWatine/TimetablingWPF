using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Reflection;
using System.Collections;
using Xceed.Wpf.Toolkit;
using System.Windows.Data;

namespace TimetablingWPF
{
    public static class GenericExtensions
    {
        public static void InsertDefaultIndex<T>(this IList<T> list, int index, T item)
        {
            list.Insert(Math.Min(list.Count, index), item);
        }
        /// <summary>
        /// Removes whitespace from a string
        /// </summary>
        /// <remarks>Taken from https://stackoverflow.com/a/30732794 </remarks>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string RemoveWhitespace(this string str)
        {
            return string.Join("", str.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
        }
        public static bool IsInterface<T>(this Type type)
        {
            return typeof(T).IsAssignableFrom(type);
        }
        public static void BindValue(this IntegerUpDown iupdown, object item, string property)
        {
            iupdown.SetBinding(IntegerUpDown.ValueProperty, new Binding(property)
            {
                Source = item,
                Mode = BindingMode.TwoWay
            });

        }
        public static void BindToProperty(this IntegerUpDown iupdown, DependencyProperty dp, object item, string property)
        {
            iupdown.SetBinding(dp, new Binding(property)
            {
                Source = item,
                Mode = BindingMode.TwoWay
            });

        }
    }
}
