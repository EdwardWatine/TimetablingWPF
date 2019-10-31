using Humanizer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimetablingWPF
{
    public static class GenericHelpers
    {
        public static void MoveElementProperties(object @in, object @out, string[] props)
        {
            foreach (string prop in props)
            {
                object holder = @in.GetType().GetProperty(prop).GetValue(@in);
                @in.GetType().GetProperty(prop).SetValue(@in, null);
                @out.GetType().GetProperty(prop).SetValue(@out, holder);
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
    }
}
