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
        public BDCSortingComparer(string target)
        {
            Target = target;
        }

        private readonly string Target;

        public int Compare(object x, object y)
        {
            string xname = ((BaseDataClass)x).Name.RemoveWhitespace().ToUpperInvariant();
            string yname = ((BaseDataClass)y).Name.RemoveWhitespace().ToUpperInvariant();
            int ymod = 0, xmod = 0;
            if (Target.Length >= 3)
            {
                if (yname.Contains(Target)) ymod = yname.Length;
                if (xname.Contains(Target)) xmod = xname.Length;
            }
            if (xname.Length < yname.Length)
            {
                int xdist = DamerauLevenshteinDistance(xname, Target);
                return xdist - xmod + ymod - DamerauLevenshteinDistance(yname, Target, xdist - xmod + ymod);
            }
            int ydist = DamerauLevenshteinDistance(yname, Target);
            return DamerauLevenshteinDistance(xname, Target, xmod + ydist - ymod) - xmod - ydist + ymod;
        }
    }
}
