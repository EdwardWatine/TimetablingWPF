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
        public static IList GenerateOneWayCopy(this INotifyCollectionChanged collection)
        {
            IList copy = new ObservableCollection<object>(((IEnumerable)collection).Cast<object>());
            collection.CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs e)
            {
                if (e.NewItems != null) { 
                    foreach (object item in e.NewItems)
                    {
                        copy.Add(item);
                    }
                }
                if (e.OldItems != null) {
                    foreach (object item in e.OldItems)
                    {
                        copy.Remove(item);
                    }
                }
            };
            return copy;
        }
        public static bool IsNotPropertyChanged(this NotifyCollectionChangedEventArgs e)
        {
            return (e.NewItems != null || e.OldItems != null) && (e.Action != NotifyCollectionChangedAction.Replace || !ReferenceEquals(e.NewItems[0], e.OldItems[0]));
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
        public static IList GenerateVisibles(this IList list)
        {
            ObservableCollection<object> collection = new ObservableCollection<object>(list.Cast<IDataObject>().Where(o => o.Visible));
            if (list is INotifyCollectionChanged changingCollection)
            {
                changingCollection.CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs e)
                {
                    if (e.OldItems != null)
                    {
                        foreach (IDataObject item in e.OldItems.Cast<IDataObject>())
                        {
                            if (item.Visible)
                            {
                                collection.Remove(item);
                            }
                        }
                    }
                    if (e.NewItems != null)
                    {
                        foreach (IDataObject item in e.NewItems.Cast<IDataObject>())
                        {
                            if (item.Visible)
                            {
                                collection.Add(item);
                            }
                        }
                    }
                };
            }
            return collection;
        }
    }
}
