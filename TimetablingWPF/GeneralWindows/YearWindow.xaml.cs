using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace TimetablingWPF
{
    /// <summary>
    /// Interaction logic for YearWindow.xaml
    /// </summary>
    public partial class YearWindow : Window
    {
        private class YearData : INotifyPropertyChanged
        {
            public YearData(Year year)
            {
                Name = year.Name;
                Deleted = false;
                Year = year;
            }
            public event PropertyChangedEventHandler PropertyChanged;
            private void RaisePropertyChanged(string property)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
            }

            public override string ToString()
            {
                return Name;
            }
            public string Name { get; set; }
            public bool Deleted { get => del; set
                {
                    if (del ^ value)
                    {
                        del = value;
                        RaisePropertyChanged("Deleted");   
                    }
                }
            }
            private bool del;
            public Year Year { get; set; }
            public EditableText Parent { get; set; }
        }
        public YearWindow()
        {
            InitializeComponent();
            lbMain.ItemsSource = YearList;
        }
        private readonly IList<YearData> YearList = new ObservableCollection<YearData>(App.Data.YearGroups.Where(y => y.Visible).Select(y => new YearData(y)));
        private bool focusFlag = false;
        private void SelectedChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbMain.SelectedItem == null || !((YearData)lbMain.SelectedItem).Deleted) {
                btDelete.Content = "Remove";
            }
            else
            {
                btDelete.Content = "Restore";
            }
        }
        private void AddYear(object sender, RoutedEventArgs e)
        {
            focusFlag = true;
            YearList.Add(new YearData(new Year("<New Year Group>")));
            lbMain.SelectedIndex = lbMain.Items.Count - 1;

        }
        private void ToggleDelete(object sender, RoutedEventArgs e)
        {
            if (lbMain.SelectedItem != null)
            {
                YearData item = (YearData)lbMain.SelectedItem;
                if (!item.Year.Committed)
                {
                    YearList.Remove(item);
                    return;
                }
                item.Deleted = !item.Deleted;
                SelectedChanged(null, null);
            }
        }
        private void EditClick(object sender, RoutedEventArgs e)
        {
            ((YearData)lbMain.SelectedItem).Parent.Focus();
        }
        private void CloseClick(object sender, RoutedEventArgs e)
        {
            if (!YearList.Any(yd => !yd.Year.Committed || yd.Deleted) ||
                VisualHelpers.ShowWarningBox("Any changes you have made will not be saved. Continue?") == MessageBoxResult.OK)
            {
                Close();
            }
        }
        private void SaveClick(object sender, RoutedEventArgs e)
        {
            bool flag = false;
            foreach (YearData data in YearList)
            {
                if (data.Deleted)
                {
                    if (!flag && data.Year.Forms.Count > 0)
                    {
                        if (VisualHelpers.ShowWarningBox("Some year groups have forms associated with them. "+
                            "Removing the year group will mean these form will no longer have a year group associated with them. Continue?") != MessageBoxResult.OK)
                        {
                            return;
                        }
                    }
                    data.Year.Delete();
                    continue;
                }
                data.Year.Name = data.Name;
                data.Year.Commit();
                Close();
            }
        }
        private void YearLoaded(object sender, RoutedEventArgs e)
        {
            EditableText parent = (EditableText)sender;
            ((YearData)parent.DataContext).Parent = parent;
            if (focusFlag)
            {
                ((EditableText)sender).Focus();
                focusFlag = false;
            }
        }
    }
}
