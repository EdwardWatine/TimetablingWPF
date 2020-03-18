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
    /// Interaction logic for YearWindow.xaml
    /// </summary>
    public partial class YearWindow : Window
    {
        public YearWindow()
        {
            InitializeComponent();
            lbMain.ItemsSource = DataHelpers.GetDataContainer().YearGroups;
        }
        private void AddYear(object sender, RoutedEventArgs e)
        {
            DataHelpers.GetDataContainer().YearGroups.Add(new YearGroup("<New Year Group>"));
            lbMain.SelectedIndex = lbMain.Items.Count - 1;

        }
        private void RemoveYear(object sender, RoutedEventArgs e)
        {
            if (lbMain.SelectedItem != null)
            {
                DataHelpers.GetDataContainer().YearGroups.Remove((YearGroup)lbMain.SelectedItem);
            }
        }
        private void CloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
