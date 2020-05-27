using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TimetablingWPF.Searching
{
    public class ListLogicModifier : ComboBox
    {
        public ListLogicModifier()
        {
            Items.Add(AnyOf);
            Items.Add(AllOf);
            SelectedIndex = 0;
            Width = 80;
        }
        public ListLogicModification? GetListLogicModification()
        {
            switch ((string)SelectedItem)
            {
                case AnyOf:
                    return ListLogicModification.AnyOf;
                case AllOf:
                    return ListLogicModification.AllOf;
                default:
                    return null;
            }
        }
        private const string AnyOf = "any of";
        private const string AllOf = "all of";
    }
    public enum ListLogicModification
    {
        AnyOf,
        AllOf
    }
}
