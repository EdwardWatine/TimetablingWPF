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
    public class ListSearch<TThis, TCollection> :  SearchBase
    {
        public ListSearch(Func<TThis, IEnumerable<TCollection>> valuesGenerator, IList optionlist, string optiontype, string verb)
        {
            Verb = verb;
            Optiontype = optiontype;
            OptionList = optionlist;
            ValuesGenerator = valuesGenerator;
        }
        private ListLogicModifier modifier;
        private MultiComboBox options;
        private readonly IList OptionList;
        private readonly Func<TThis, IEnumerable<TCollection>> ValuesGenerator;
        public override UIElement GenerateUI()
        {
            modifier = new ListLogicModifier();
            options = new MultiComboBox()
            {
                ItemString = Optiontype,
                ItemsSource = OptionList
            };
            StackPanel spMain = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
            };
            spMain.Children.Add(new TextBlock() { Text = Verb });
            spMain.Children.Add(modifier);
            spMain.Children.Add(options);
            return spMain;
        }

        public override bool Search(object item)
        {
            IEnumerable<TCollection> values = ValuesGenerator((TThis)item);
            if (modifier.GetListLogicModification() == ListLogicModification.AnyOf)
            {
                return options.SelectedItems.AsParallel().Cast<TCollection>().Any(g => values.Contains(g));
            }
            return !options.SelectedItems.AsParallel().Cast<TCollection>().Any(g => !values.Contains(g));
        }
    }
}
