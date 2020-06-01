using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TimetablingWPF.Searching;
using static TimetablingWPF.GenericHelpers;

namespace TimetablingWPF
{
    public class SortingComparer : IComparer, IComparer<object>
    {
        public string Filter { get; set; }
        public Func<object, string> StringFunction { get; set; } = o => o.ToString();

        public int Compare(object x, object y)
        {
            string xname = StringFunction(x).RemoveWhitespace().ToUpperInvariant();
            string yname = StringFunction(y).RemoveWhitespace().ToUpperInvariant();
            if (xname.Length < yname.Length)
            {
                int xdist = DamerauLevenshteinDistance(xname, Filter);
                return xdist - DamerauLevenshteinDistance(yname, Filter, xdist);
            }
            int ydist = DamerauLevenshteinDistance(yname, Filter);
            return DamerauLevenshteinDistance(xname, Filter) - ydist;
        }
    }
}
