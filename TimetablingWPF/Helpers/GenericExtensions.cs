using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TimetablingWPF
{
    public static class GenericExtensions
    {
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }
        public static string Join<T>(this string str, IEnumerable<T> enumerable)
        {
            return string.Join(str, enumerable.Select(t => Convert.ToString(t, GenericHelpers.GetGlobalCultureInfo())));
        }

        public static void RefreshSelected(this TabControl tc)
        {
            int i = tc.SelectedIndex;
            tc.SelectedIndex = -1;
            tc.SelectedIndex = i;
        }
    }
}
