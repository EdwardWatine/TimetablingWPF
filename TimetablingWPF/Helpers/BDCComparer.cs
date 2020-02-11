using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TimetablingWPF.GenericHelpers;

namespace TimetablingWPF
{
    public class BDCSortingComparer : IComparer
    {
        public string Filter { get; set; }

        public int Compare(object x, object y)
        {
            string xname = ((BaseDataClass)x).Name.RemoveWhitespace().ToUpperInvariant();
            string yname = ((BaseDataClass)y).Name.RemoveWhitespace().ToUpperInvariant();
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
