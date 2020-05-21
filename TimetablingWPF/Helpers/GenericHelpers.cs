using Humanizer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TimetablingWPF;

namespace TimetablingWPF
{
    public static class GenericHelpers
    {
        public static SolidColorBrush FromRGBA(byte R, byte G, byte B, byte A = 255)
        {
            return new SolidColorBrush(Color.FromArgb(A, R, G, B));
        }
        public static NotifyCollectionChangedEventHandler GenerateLinkHandler(IList list)
        {
            return new NotifyCollectionChangedEventHandler((sender, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (object obj in e.NewItems)
                    {
                        list.Add(obj);
                    }
                }
                if (e.OldItems != null)
                {
                    foreach (object obj in e.OldItems)
                    {
                        list.Remove(obj);
                    }
                }
            });
        }
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
            return number == 1 ? word.Singularize(false) : word.Pluralize(false);
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

        public static ErrorContainer GenerateNameError(TextBox box, string type)
        {
            ErrorContainer ec = new ErrorContainer((e) => string.IsNullOrWhiteSpace(box.Text), (e) => $"{type} has no name.",
                ErrorType.Error);
            box.TextChanged += delegate (object sender, TextChangedEventArgs e) { ec.UpdateError(); };
            return ec;
        }

        public static void CanAlwaysExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        /// <summary>
        /// Computes the Damerau-Levenshtein Distance between two strings, represented as arrays of
        /// integers, where each integer represents the code point of a character in the source string.
        /// Includes an optional threshhold which can be used to indicate the maximum allowable distance.
        /// </summary>
        /// <remarks>Taken from: https://stackoverflow.com/a/9454016 </remarks>
        /// <param name="source">An array of the code points of the first string</param>
        /// <param name="target">An array of the code points of the second string</param>
        /// <param name="threshold">Maximum allowable distance</param>
        /// <returns>Int.MaxValue if threshhold exceeded; otherwise the Damerau-Leveshteim distance between the strings</returns>
        public static int DamerauLevenshteinDistance(string source, string target, int threshold = int.MaxValue)
        {

            int length1 = source.Length;
            int length2 = target.Length;

            // Return trivial case - difference in string lengths exceeds threshhold
            if (Math.Abs(length1 - length2) > threshold) { return int.MaxValue; }
            if (source == target) return 0;

            // Ensure arrays [i] / length1 use shorter length 
            if (length1 > length2)
            {
                Swap(ref target, ref source);
                Swap(ref length1, ref length2);
            }

            int maxi = length1;
            int maxj = length2;

            int[] dCurrent = new int[maxi + 1];
            int[] dMinus1 = new int[maxi + 1];
            int[] dMinus2 = new int[maxi + 1];
            int[] dSwap;

            for (int i = 0; i <= maxi; i++) { dCurrent[i] = i; }

            int jm1 = 0;

            for (int j = 1; j <= maxj; j++)
            {

                // Rotate
                dSwap = dMinus2;
                dMinus2 = dMinus1;
                dMinus1 = dCurrent;
                dCurrent = dSwap;

                // Initialize
                int minDistance = int.MaxValue;
                dCurrent[0] = j;
                int im1 = 0;
                int im2 = -1;
                for (int i = 1; i <= maxi; i++)
                {

                    int cost = source[im1] == target[jm1] ? 0 : 1;

                    int del = dCurrent[im1] + 1;
                    int ins = dMinus1[i] + 1;
                    int sub = dMinus1[im1] + cost;

                    //Fastest execution for min value of 3 integers
                    int min = (del > ins) ? (ins > sub ? sub : ins) : (del > sub ? sub : del);

                    if (i > 1 && j > 1 && source[im2] == target[jm1] && source[im1] == target[j - 2])
                        min = Math.Min(min, dMinus2[im2] + cost);

                    dCurrent[i] = min;
                    if (min < minDistance) { minDistance = min; }
                    im1++;
                    im2++;
                }
                jm1++;
                if (minDistance > threshold) { return int.MaxValue; }
            }

            int result = dCurrent[maxi];
            return (result > threshold) ? int.MaxValue : result;
        }
        static void Swap<T>(ref T arg1, ref T arg2)
        {
            T temp = arg1;
            arg1 = arg2;
            arg2 = temp;
        }
    }
}
