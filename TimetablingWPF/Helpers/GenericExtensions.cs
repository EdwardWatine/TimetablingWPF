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
    }
}
