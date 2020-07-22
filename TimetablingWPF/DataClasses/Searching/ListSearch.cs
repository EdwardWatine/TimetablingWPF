using Humanizer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TimetablingWPF.Searching
{
    public class ListSearchFactory<TThis, TCollection> : SearchFactory
    {
        public ListSearchFactory(Func<TThis, IEnumerable<TCollection>> valuesGenerator, IList optionlist, string optiontype, string verb)
        {
            ListSearch.Verb = verb;
            ListSearch.Optiontype = optiontype;
            ListSearch.OptionList = optionlist;
            ListSearch.ValuesGenerator = valuesGenerator;
        }
        
        public override SearchBase GenerateSearch()
        {
            return new ListSearch();
        }
        private class ListSearch : SearchBase
        {
            private ListLogicModifier modifier;
            private MultiComboBox options;
            public static IList OptionList;
            public static Func<TThis, IEnumerable<TCollection>> ValuesGenerator;
            public static string Verb;
            public static string Optiontype;
            public override UIElement GenerateUI()
            {
                Thickness sep = new Thickness(5, 0, 0, 0);
                modifier = new ListLogicModifier()
                {
                    Margin = sep,
                    VerticalContentAlignment = VerticalAlignment.Center
                };
                modifier.SelectionChanged += GenerateSelectionChangedHandler("modifier");
                options = new MultiComboBox()
                {
                    ItemString = Optiontype,
                    ItemsSource = OptionList,
                    Margin = sep
                };
                options.SelectionChanged += GenerateSelectionChangedHandler("options");
                StackPanel spMain = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 0, 30, 0),
                };
                spMain.Children.Add(new TextBlock() { Text = Verb, VerticalAlignment = VerticalAlignment.Center});
                spMain.Children.Add(modifier);
                spMain.Children.Add(new TextBlock() { Text = Optiontype.Pluralize() + ":", VerticalAlignment = VerticalAlignment.Center, Margin = sep });
                spMain.Children.Add(options);
                return spMain;
            }

            public override bool Search(object item)
            {
                HashSet<TCollection> values = new HashSet<TCollection>(ValuesGenerator((TThis)item));
                if (options.SelectedItems.Count == 0)
                {
                    return true;
                }
                if (modifier.GetListLogicModification() == ListLogicModification.AnyOf)
                {
                    return options.SelectedItems.AsParallel().Cast<TCollection>().Any(g => values.Contains(g));
                }
                return options.SelectedItems.AsParallel().Cast<TCollection>().All(g => values.Contains(g));
            }
        }
    }
}
