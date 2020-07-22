using Humanizer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit;

namespace TimetablingWPF.Searching
{
    public class ComparisonSearchFactory<T> : SearchFactory
    {
        public ComparisonSearchFactory(Func<T, int> valueGenerator, string verb)
        {
            ListSearch.Verb = verb;
            ListSearch.ValueGenerator = valueGenerator;
        }
        
        public override SearchBase GenerateSearch()
        {
            return new ListSearch();
        }
        private class ListSearch : SearchBase
        {
            private ComparisonLogicModifier modifier;
            private IntegerUpDown target;
            public static IList OptionList;
            public static Func<T, int> ValueGenerator;
            public static string Verb;
            public static string Optiontype;
            public override UIElement GenerateUI()
            {
                Thickness sep = new Thickness(5, 0, 0, 0);
                modifier = new ComparisonLogicModifier()
                {
                    Margin = sep,
                    VerticalContentAlignment = VerticalAlignment.Center
                };
                modifier.SelectionChanged += GenerateSelectionChangedHandler("modifier");
                target = new IntegerUpDown()
                {
                    Value = 0,
                    Minimum = 0
                };
                StackPanel spMain = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 0, 30, 0),
                };
                spMain.Children.Add(new TextBlock() { Text = Verb, VerticalAlignment = VerticalAlignment.Center});
                spMain.Children.Add(modifier);
                spMain.Children.Add(target);
                return spMain;
            }

            public override bool Search(object item)
            {
                int value = ValueGenerator((T)item);
                int target_val = target.Value ?? 0;
                ComparisonModification mod = modifier.GetListLogicModification() ?? ComparisonModification.GreaterEq;
                switch (mod)
                {
                    case ComparisonModification.Greater:
                        return value > target_val;
                    case ComparisonModification.Lower:
                        return value < target_val;
                    case ComparisonModification.GreaterEq:
                        return value >= target_val;
                    case ComparisonModification.LowerEq:
                        return value <= target_val;
                    default:
                        return value == target_val;
                }

            }
        }
    }
}
