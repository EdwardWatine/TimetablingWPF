using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TimetablingWPF
{
    /// <summary>
    /// Interaction logic for CheckboxDialog.xaml
    /// </summary>
    public partial class CheckboxDialog : Window
    {
        int NumBoxes;

        public CheckboxDialog(Window owner, string[] checkboxes)
        {
            Owner = owner;

            NumBoxes = checkboxes.Length;
            
            InitializeComponent();
            foreach (string text in checkboxes)
            {
                if (text == "\n")
                {
                    spCheckboxes.Children.Add(new Separator());
                    continue;
                }
                CheckBox newCBox = new CheckBox
                {
                    Content = new TextBlock
                    {
                        Text = text,
                        Style = (Style)Application.Current.Resources["DialogText"]
                    },
                    IsChecked = true
                };
                spCheckboxes.Children.Add(newCBox);
            }

        }

        private void Confirm(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        public bool[] Result
        {
            get
            {

                bool[] result = new bool[NumBoxes];
                int seps = 0;
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(spCheckboxes); i++)
                {
                    Visual visual = (Visual)VisualTreeHelper.GetChild(spCheckboxes, i);

                    if (visual.GetType() == typeof(Separator))
                    {
                        seps++;
                        continue;
                    }

                    result[i-seps] = (bool)visual.GetValue(CheckBox.IsCheckedProperty);
                }
                return result;
            }
        }
    }
}
