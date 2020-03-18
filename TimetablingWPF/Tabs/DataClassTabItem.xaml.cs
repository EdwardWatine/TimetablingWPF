using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
            void attachButtonCommand(ButtonBase item, CommandBinding binding, // local function to assist attaching commands
                object parameter = null)
            {
                item.CommandParameter = parameter;
                item.Command = binding.Command;
                item.CommandBindings.Add(binding);
            }
            void attachMICommand(MenuItem item, CommandBinding binding, // local function to assist attaching commands
                object parameter = null)
            {
                item.CommandParameter = parameter;
                item.Command = binding.Command;
                item.CommandBindings.Add(binding);
            }
            CommandBinding editBinding = new CommandBinding(DataGridCommands.EditItem, ExecuteEditItem, CanExecuteEditItem);
            CommandBinding newBinding = new CommandBinding(DataGridCommands.NewItem, ExecuteNewItem, CanAlwaysExecute);
            CommandBinding dupBinding = new CommandBinding(DataGridCommands.DuplicateItem, ExecuteDuplicateItem, CanExecuteDuplicateItem);
            CommandBinding delBinding = new CommandBinding(DataGridCommands.DeleteItem, ExecuteDeleteItem, CanExecuteDeleteItem);

            attachMICommand(miEditItem, editBinding);
            attachMICommand(miNewItem, newBinding);
            attachMICommand(miDeleteItem, delBinding);     // attach commands to the context menu
            attachMICommand(miDuplicateItem, dupBinding);

            dgMainDataGrid.CommandBindings.Add(delBinding);
            dgMainDataGrid.InputBindings.Add(new KeyBinding(DataGridCommands.DeleteItem, new KeyGesture(Key.Delete)));

            attachButtonCommand(btNewToolbar, newBinding);
            attachButtonCommand(btEditToolbar, editBinding);
            attachButtonCommand(btDuplicateToolbar,dupBinding); // attach commands to the toolbar
            attachButtonCommand(btDeleteToolbar, delBinding);

            filterName.TextChanged += delegate (object sender, TextChangedEventArgs e) { RefreshFilter(); };
            IList data = DataHelpers.GetDataContainer().FromType(type);
            ListCollectionView view = new ListCollectionView(data);
            ((INotifyCollectionChanged)data).CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs e) { view.Refresh(); };
            dgMainDataGrid.ItemsSource = view;
            dgMainDataGrid.Columns.Add(new DataGridTemplateColumn()
            {
                CanUserSort = true,
                SortMemberPath = "Name",
                Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                MinWidth = 20,
                Header = "Name",
                CellTemplate = (DataTemplate)Resources["NameTemplate"]
            });
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);
            int width = BaseDataClass.ExposedProperties[type].Count(prop => prop.PropertyInfo.PropertyType.IsInterface<IList>());
            width = width == 1 ? 4 : width;
            foreach (CustomPropertyInfo prop in BaseDataClass.ExposedProperties[type])
            {
                bool islist = prop.PropertyInfo.PropertyType.IsInterface<IList>();
                DataTemplate cellTemplate = new DataTemplate(type);
                FrameworkElementFactory tbFactory = new FrameworkElementFactory(typeof(TextBlock));
                tbFactory.SetValue(StyleProperty, Resources["tbStyle"]);
                Binding binding = new Binding(prop.PropertyInfo.Name);
                if (prop.PropertyInfo.PropertyType == typeof(ObservableCollection<TimetableSlot>))
                {
                    binding.Converter = new ListReportLength();
                    tbFactory.SetBinding(ToolTipProperty, new Binding(prop.PropertyInfo.Name)
                    {
                        Converter = new PeriodsToTable()
                    });
                }
                else if (islist)
                {
                    binding.Converter = new ListFormatter();
                }
                else
                {
                    binding.Converter = new PropertyConverter()
                    {
                        CustomConverter = o => prop.Display(o)
                    };
                }
                tbFactory.SetBinding(TextBlock.TextProperty, binding);
                cellTemplate.VisualTree = tbFactory;
                
                dgMainDataGrid.Columns.Add(new DataGridTemplateColumn()
                {
                    Width = new DataGridLength(width, islist ? DataGridLengthUnitType.Star : DataGridLengthUnitType.Auto),
                    Header = prop.Alias,
                    CellTemplate = cellTemplate
                });
            }
            dgMainDataGrid.MouseDoubleClick += delegate (object sender, MouseButtonEventArgs e)
            {
                if (e.LeftButton == MouseButtonState.Pressed && dgMainDataGrid.SelectedItems.Count == 1)
                {
                    new InformationWindow((BaseDataClass)dgMainDataGrid.SelectedItem).Show();
                }
            };
            dgMainDataGrid.UnselectAll();
        }
        
        public Type DataType { get; }
        private readonly BDCSortingComparer FilterComparer = new BDCSortingComparer();
        private void ExecuteNewItem(object sender, ExecutedRoutedEventArgs e)
        {
            MainPage.NewTab(DataHelpers.GenerateItemTab(Activator.CreateInstance(DataType), CommandType.@new), $"New {DataType.Name}");
        }

        private void ExecuteEditItem(object sender, ExecutedRoutedEventArgs e)
        {
            object item = dgMainDataGrid.SelectedItem; // edit item tab
            MainPage.NewTab(DataHelpers.GenerateItemTab(item, CommandType.edit), $"Edit {item.GetType().Name}");

        }

        private void CanExecuteEditItem(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = dgMainDataGrid.SelectedItems.Count == 1;// && dgMainDataGrid.IsFocused;  grid is selected
        }

        private void ExecuteDuplicateItem(object sender, ExecutedRoutedEventArgs e)
        {
            object item = dgMainDataGrid.SelectedItem; // duplicate item tab
            MainPage.NewTab(DataHelpers.GenerateItemTab(item, CommandType.copy), $"New {item.GetType().Name}");
        }

        private void CanExecuteDuplicateItem(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = dgMainDataGrid.SelectedItems.Count == 1;// && dgMainDataGrid.IsFocused;  // grid is selected
        }

        private void ExecuteDeleteItem(object sender, ExecutedRoutedEventArgs e)
        {
            int num_sel = dgMainDataGrid.SelectedItems.Count;
            string conf_str = num_sel == 1 ? $"'{((BaseDataClass)dgMainDataGrid.SelectedItem).Name}'" : $"{num_sel} {dgMainDataGrid.Tag}";
            if (VisualHelpers.ShowWarningBox("Are you sure you want to delete " + conf_str + "?", $"Delete {conf_str}?") == MessageBoxResult.Cancel) return;
            IList<BaseDataClass> list = dgMainDataGrid.SelectedItems.Cast<BaseDataClass>().ToList();
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Delete(); // deletes items
            }
        }

        private void CanExecuteDeleteItem(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = dgMainDataGrid.SelectedItems.Count >= 1;// && dgMainDataGrid.IsFocused; // at least one item is selected
        }

        public void ExecuteToggleFilter()
        {
            bool visible = spFilter.Visibility == Visibility.Visible;
            if (visible)
            {
                spFilter.Visibility = Visibility.Collapsed;
                RefreshFilter();
                return;
            }
            spFilter.Visibility = Visibility.Visible;
            RefreshFilter();
            filterName.Focus();
        }
        public void RefreshFilter()
        {
            string nameFilter = filterName.Text.RemoveWhitespace().ToUpperInvariant();
            ListCollectionView data = (ListCollectionView)dgMainDataGrid.ItemsSource;
            if (string.IsNullOrWhiteSpace(nameFilter) || spFilter.Visibility != Visibility.Visible)
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

    public enum CommandType // specifies the behaviour of the tab
    {
        @new,
        edit,
        copy
    }
}
