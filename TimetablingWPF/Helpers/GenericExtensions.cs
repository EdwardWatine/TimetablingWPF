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
using System.Collections.Specialized;
using System.ComponentModel;
using ObservableComputations;

namespace TimetablingWPF
{
    public static class GenericExtensions
    {
        public static ObservableCollectionExtended<T> ToObservable<T>(this IEnumerable<T> enumerable)
        {
            var temp = new ObservableCollectionExtended<T>();
            temp.AddRange(enumerable);
            return temp;
        }
        public static void AddRange(this IList list, IEnumerable range)
        {
            foreach (object item in range)
            {
                list.Add(item);
            }
        }
        public static void AddRange<T>(this IList<T> list, IEnumerable<T> range)
        {
            foreach (T item in range)
            {
                list.Add(item);
            }
        }
        public static void SetData(this IList list, IEnumerable data)
        {
            list.Clear();
            list.AddRange(data);
        }
        public static void SetData<T>(this IList<T> list, IEnumerable<T> data)
        {
            list.Clear();
            list.AddRange(data);
        }
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
        public static void Insert(this Grid grid, UIElement element, int row, int col)
        {
            Grid.SetColumn(element, (grid.ColumnDefinitions.Count + col) % grid.ColumnDefinitions.Count);
            Grid.SetRow(element, (grid.RowDefinitions.Count + row) % grid.RowDefinitions.Count);
            grid.Children.Add(element);
        }
        public static void SmartInsert(this IList list, int index, object element)
        {
            int nindex = (list.Count + index) % list.Count;
            if (index < 0) nindex += 1;
            list.Insert(nindex, element);
        }
        public static string InsertArticle(this string str)
        {
            return AvsAnLib.AvsAn.Query(str).Article + " " + str;
        }
        public static void LinkList(this INotifyCollectionChanged collection, IList target)
        {
            collection.CollectionChanged += GenericHelpers.GenerateLinkHandler(target);
        }
        public static ObservableCollectionExtended<object> GenerateOneWayCopy(this INotifyCollectionChanged collection)
        {
            ObservableCollectionExtended<object> copy = ((IEnumerable)collection).Cast<object>().ToObservable();
            collection.LinkList(copy);
            return copy;
        }
        public static bool IsAddOrRemove(this NotifyCollectionChangedEventArgs e)
        {
            return (e.NewItems != null || e.OldItems != null) && (e.Action != NotifyCollectionChangedAction.Replace || !ReferenceEquals(e.NewItems[0], e.OldItems[0])) && (e.Action != NotifyCollectionChangedAction.Move);
        }
        public static bool IsReplace(this NotifyCollectionChangedEventArgs e)
        {
            return e.NewItems != null && e.OldItems != null && ReferenceEquals(e.NewItems[0], e.OldItems[0]);
        }
        public static void DefaultDictGet<TKey, TValue, TDefault>(this Dictionary<TKey, TValue> dict, TKey key, out TValue value) where TDefault : TValue, new()
        {
            if (!dict.TryGetValue(key, out value))
            {
                value = new TDefault();
                dict[key] = value;
            }
        }

        public static TValue DefaultDictGet<TKey, TValue, TDefault>(this Dictionary<TKey, TValue> dict, TKey key) where TDefault : TValue, new()
        {
            dict.DefaultDictGet<TKey, TValue, TDefault>(key, out TValue value);
            return value;
        }
        public static void DefaultDictGet<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, out TValue value) where TValue : new()
        {
            dict.DefaultDictGet<TKey, TValue, TValue>(key, out value);
        }
        public static System.Windows.Media.Color ToMediaColor(this System.Drawing.Color color)
        {
            return System.Windows.Media.Color.FromRgb(color.R, color.G, color.B);
        }
        public static System.Drawing.Color ToDrawingColor(this System.Windows.Media.Color color)
        {
            return System.Drawing.Color.FromArgb(color.R, color.G, color.B);
        }
    }
}
