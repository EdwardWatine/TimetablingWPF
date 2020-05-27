using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            int ymod = 0, xmod = 0;
            if (Filter.Length >= 3)
            {
                if (yname.Contains(Filter)) ymod = yname.Length;
                if (xname.Contains(Filter)) xmod = xname.Length;
            }
            if (xname.Length < yname.Length)
            {
                int xdist = DamerauLevenshteinDistance(xname, Filter);
                return xdist - xmod + ymod - DamerauLevenshteinDistance(yname, Filter, xdist - xmod + ymod);
            }
            int ydist = DamerauLevenshteinDistance(yname, Filter);
            return DamerauLevenshteinDistance(xname, Filter, xmod + ydist - ymod) - xmod - ydist + ymod;
        }
    }
}
