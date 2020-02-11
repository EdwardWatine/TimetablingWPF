using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using Humanizer;
using static TimetablingWPF.GenericHelpers;

namespace TimetablingWPF
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class DataClassTabItem : TabBase
    {
        public DataClassTabItem(Type type)
        {
            InitializeComponent();
            DataType = type;
            void attachMenuCommand(string key, CommandBinding binding,
                object parameter = null)
            {
                ((MenuItem)Resources[key]).CommandParameter = parameter;
                ((MenuItem)Resources[key]).CommandBindings.Add(binding);
                ((MenuItem)Resources[key]).Command = binding.Command;
            }
            void attachButtonCommand(Button button, CommandBinding binding,
                object parameter = null)
            {
                button.CommandParameter = parameter;
                button.CommandBindings.Add(binding);
                button.Command = binding.Command;
            }
            CommandBinding editBinding = new CommandBinding(DataGridCommands.EditItem, ExecuteEditItem, CanExecuteEditItem);
            CommandBinding newBinding = new CommandBinding(DataGridCommands.NewItem, ExecuteNewItem, CanAlwaysExecute);
            CommandBinding dupBinding = new CommandBinding(DataGridCommands.DuplicateItem, ExecuteDuplicateItem, CanExecuteDuplicateItem);
            CommandBinding delBinding = new CommandBinding(DataGridCommands.DeleteItem, ExecuteDeleteItem, CanExecuteDeleteItem);

            attachMenuCommand($"miEditItem", editBinding);
            attachMenuCommand($"miNewItem", newBinding);
            attachMenuCommand($"miDeleteItem", delBinding);
            attachMenuCommand($"miDuplicateItem", dupBinding);

            dgMainDataGrid.CommandBindings.Add(delBinding);
            dgMainDataGrid.InputBindings.Add(new KeyBinding(DataGridCommands.DeleteItem, new KeyGesture(Key.Delete)));

            attachButtonCommand(btNewToolbar, newBinding);
            attachButtonCommand(btEditToolbar, editBinding);
            attachButtonCommand(btDuplicateToolbar,dupBinding);
            attachButtonCommand(btDeleteToolbar, delBinding);

            filterName.TextChanged += delegate (object sender, TextChangedEventArgs e) { RefreshFilter(); };

            dgMainDataGrid.ItemsSource = new ListCollectionView(DataHelpers.GetDataContainer().FromType(type));
            dgMainDataGrid.Columns.Add(new DataGridTemplateColumn()
            {
                CanUserSort = true,
                SortMemberPath = "Name",
                Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                MinWidth = 20,
                Header = "Name",
                CellTemplate = (DataTemplate)Resources["NameTemplate"]
            });
            int width = new HashSet<string>(Columns[type.Name]).Except(Shortcols).Count();
            width = width == 1 ? 4 : width;
            foreach (string column in Columns[type.Name])
            {
                dgMainDataGrid.Columns.Add(new DataGridTemplateColumn()
                {
                    Width = new DataGridLength(width, Shortcols.Contains(column) ? DataGridLengthUnitType.Auto : DataGridLengthUnitType.Star),
                    Header = column,
                    CellTemplate = (DataTemplate)Resources[$"{column}Template"]
                });
            }
            dgMainDataGrid.UnselectAll();
        }
        
        public Type DataType { get; }
        private readonly Dictionary<string, string[]> Columns = new Dictionary<string, string[]>()
            {
                { "Teacher", new string[]{"Subjects", "Assignments", "Unavailable Periods" } },
                { "Subject", new string[]{"Teachers", "Groups" } },
                { "Lesson", new string[]{"Subject", "Lessons Per Cycle", "Lesson Length", "Assignments", "Forms" } },
                { "Group", new string[]{"Subjects", "Rooms" } },
                { "Form", new string[]{"Year Group", "Lessons" } },
                { "Room", new string[]{"Quantity", "Critical", "Groups"} }
            };
        private readonly HashSet<string> Shortcols = new HashSet<string>() { "Lessons Per Cycle", "Lesson Length", "Quantity", "Critical", "Subject", "Year Group" };
        private readonly BDCSortingComparer FilterComparer = new BDCSortingComparer();
        private void ExecuteNewItem(object sender, ExecutedRoutedEventArgs e)
        {
            MainPage.NewTab(DataHelpers.GenerateItemTab(Activator.CreateInstance(DataType), CommandType.@new), $"New {DataType.Name}");
        }

        private void ExecuteEditItem(object sender, ExecutedRoutedEventArgs e)
        {
            object item = dgMainDataGrid.SelectedItem;
            MainPage.NewTab(DataHelpers.GenerateItemTab(item, CommandType.edit), $"Edit {item.GetType().Name}");

        }

        private void CanExecuteEditItem(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = dgMainDataGrid.SelectedItems.Count == 1 && dgMainDataGrid.IsFocused;
        }

        private void ExecuteDuplicateItem(object sender, ExecutedRoutedEventArgs e)
        {
            object item = dgMainDataGrid.SelectedItem;
            MainPage.NewTab(DataHelpers.GenerateItemTab(item, CommandType.copy), $"New {item.GetType().Name}");
        }

        private void CanExecuteDuplicateItem(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = dgMainDataGrid.SelectedItems.Count == 1 && dgMainDataGrid.IsFocused;
        }

        private void ExecuteDeleteItem(object sender, ExecutedRoutedEventArgs e)
        {
            int num_sel = dgMainDataGrid.SelectedItems.Count;
            string conf_str = num_sel == 1 ? $"'{((BaseDataClass)dgMainDataGrid.SelectedItem).Name}'" : $"{num_sel} {dgMainDataGrid.Tag}";
            if (VisualHelpers.ShowWarningBox("Are you sure you want to delete " + conf_str + "?", $"Delete {conf_str}?") == MessageBoxResult.Cancel) return;
            IList<BaseDataClass> list = dgMainDataGrid.SelectedItems.Cast<BaseDataClass>().ToList();
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Delete();
            }
        }

        private void CanExecuteDeleteItem(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = dgMainDataGrid.SelectedItems.Count >= 1 && dgMainDataGrid.IsFocused;
        }

        public void ExecuteToggleFilter()
        {
            bool visible = spFilter.Visibility == Visibility.Visible;
            if (visible)
            {
                spFilter.Visibility = Visibility.Collapsed;
                return;
            }
            spFilter.Visibility = Visibility.Visible;
            filterName.Focus();
        }
        public void RefreshFilter()
        {
            string nameFilter = filterName.Text.RemoveWhitespace().ToUpperInvariant();
            ListCollectionView data = (ListCollectionView)dgMainDataGrid.ItemsSource;
            if (string.IsNullOrWhiteSpace(nameFilter))
            {
                data.Filter = null;
                data.CustomSort = null;
                return;
            }

            data.Filter = new Predicate<object>(o => {
                string name = ((BaseDataClass)o).Name.RemoveWhitespace().ToUpperInvariant();
                bool contains = name.Contains(nameFilter);
                if (nameFilter.Length < name.Length)
                {
                    name = name.Substring(0, nameFilter.Length);
                }
                return contains || DamerauLevenshteinDistance(name, nameFilter, (nameFilter.Length + 1) / 2) != int.MaxValue;
            });
            FilterComparer.Filter = nameFilter;
            data.CustomSort = FilterComparer;
        }
        public override bool Cancel()
        {
            return true;
        }
    }
    public static class DataGridCommands
    {
        public static readonly RoutedUICommand NewItem = new RoutedUICommand(
            "NewTeacher", "NewTeacher", typeof(DataGridCommands));
        public static readonly RoutedUICommand EditItem = new RoutedUICommand(
            "EditItem", "EditItem", typeof(DataGridCommands));
        public static readonly RoutedUICommand DuplicateItem = new RoutedUICommand(
            "DuplicateItem", "DuplicateItem", typeof(DataGridCommands));
        public static readonly RoutedUICommand DeleteItem = new RoutedUICommand(
            "DeleteItem", "DeleteItem", typeof(DataGridCommands));
        public static readonly RoutedUICommand ToggleFilter = new RoutedUICommand(
            "ToggleFilter", "ToggleFilter", typeof(DataGridCommands));
    }

    public enum CommandType
    {
        @new,
        edit,
        copy
    }
}
