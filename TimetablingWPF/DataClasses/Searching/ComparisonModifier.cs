using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TimetablingWPF.Searching
{
    public class ComparisonLogicModifier : ComboBox
    {
        public ComparisonLogicModifier()
        {
            Items.Add(Greater);
            Items.Add(GreaterEq);
            Items.Add(Eq);
            Items.Add(LowerEq);
            Items.Add(Lower);
            SelectedIndex = 1;
            Width = 40;
        }
        public ComparisonModification? GetListLogicModification()
        {
            switch ((string)SelectedItem)
            {
                case Greater:
                    return ComparisonModification.Greater;
                case GreaterEq:
                    return ComparisonModification.GreaterEq;
                case Eq:
                    return ComparisonModification.Eq;
                case LowerEq:
                    return ComparisonModification.LowerEq;
                case Lower:
                    return ComparisonModification.Lower;
                default:
                    return null;
            }
        }
        private const string Greater = ">";
        private const string GreaterEq = "≥";
        private const string Eq = "=";
        private const string LowerEq = "≤";
        private const string Lower = "<";
    }
    public enum ComparisonModification
    {
        Greater,
        GreaterEq,
        Eq,
        LowerEq,
        Lower
    }
}
