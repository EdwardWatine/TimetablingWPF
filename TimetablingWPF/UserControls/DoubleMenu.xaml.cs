using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TimetablingWPF
{
    /// <summary>
    /// Interaction logic for DoubleMenu.xaml
    /// </summary>
    public partial class DoubleMenu : UserControl
    {
        public DoubleMenu()
        {
            InitializeComponent();
            lbCurrent.ItemsSource = _Result;
            lbCurrent.View = grid;
        }
        public IEnumerable<object> Dataset
        {
            set => lbAll.ItemsSource = value;
        }
        private void UpdateColumns()
        {
            grid.Columns.Clear();
            if (Columns == 1)
            {
                grid.Columns.Add(new GridViewColumn()
                {
                    DisplayMemberBinding = new Binding(),
                });
                return;
            }
            for (int i = 0; i < Columns; i++)
            {
                GridViewColumn col = new GridViewColumn()
                {
                    DisplayMemberBinding = new Binding($"Item{i + 1}")
                };
                grid.Columns.Add(col);
            }
        }
        public Func<object, object> function = x => x;
        private GridView grid = new GridView() { ColumnHeaderContainerStyle = (Style)Application.Current.Resources["Collapsed"] };
        private ObservableCollection<object> _Result = new ObservableCollection<object>();
        public IList Result => _Result;
        private int _Columns;
        public int Columns
        {
            get
            {
                return _Columns;
            }
            form
            {
                _Columns = value;
                UpdateColumns();
            }
        }

        private void RemoveDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Remove(sender, null);
        }

        private void AddDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Add(sender, null);
        }

        private void Add(object sender, RoutedEventArgs e)
        {
            foreach (object @object in lbAll.SelectedItems.Cast<object>().Except(_Result))
            {
                bool found = false;
                for (int i = 0; i < _Result.Count; i++)
                {
                    if (_Result[i].ToString().CompareTo(@object.ToString()) == 1)
                    {
                        _Result.Insert(i, function(@object));
                        found = true;
                        break;
                    }
                }
                if (!found) { _Result.Add(function(@object)); }
            }
        }

        private void Remove(object sender, RoutedEventArgs e)
        {
            foreach (object @object in lbCurrent.SelectedItems.Cast<object>().ToList())
            {
                _Result.Remove(@object);
            }
        }
    }
}
