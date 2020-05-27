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
using TimetablingWPF.Searching;
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
            filterSh.TextChanged += delegate (object sender, TextChangedEventArgs e) { RefreshFilter(); };
            cbRemove.Click += delegate (object sender, RoutedEventArgs e) { RefreshFilter(); };
            IList data = DataHelpers.GetDataContainer().FromType(type);
            defaultView = new ListCollectionView(data);
            ((INotifyCollectionChanged)data).CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs e) { defaultView.Refresh(); };
            dgMainDataGrid.ItemsSource = defaultView;
            dgMainDataGrid.Columns.Add(new DataGridTemplateColumn()
            {
                CanUserSort = true,
                SortMemberPath = "Name",
                Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                MinWidth = 20,
                Header = "Name",
                CellTemplate = (DataTemplate)Resources["NameTemplate"]
            });
            dgMainDataGrid.Columns.Add(new DataGridTemplateColumn()
            {
                CanUserSort = true,
                SortMemberPath = "Shorthand",
                Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                MinWidth = 20,
                Header = "Shorthand",
                CellTemplate = (DataTemplate)Resources["ShTemplate"]
            });
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);
            int width = BaseDataClass.ExposedProperties[type].Count(prop => prop.Type.IsInterface<IList>());
            width = width == 1 ? 4 : width;
            foreach (CustomPropertyInfo prop in BaseDataClass.ExposedProperties[type])
            {
                bool islist = prop.Type.IsInterface<IList>();
                DataTemplate cellTemplate = new DataTemplate(type);
                FrameworkElementFactory tbFactory = new FrameworkElementFactory(typeof(TextBlock));
                tbFactory.SetValue(StyleProperty, Resources["tbStyle"]);
                Binding binding = new Binding(prop.PropertyInfo.Name);
                if (prop.Type == typeof(ObservableCollection<TimetableSlot>))
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
                    binding.Converter = new PropertyConverter(prop);
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
            searches = new InternalObservableCollection<SearchBase>(BaseDataClass.SearchParameters.DefaultDictGet<Type, IList<SearchFactory>, List<SearchFactory>>(type).Select(sf => sf.GenerateSearch()).ToList());
            searches.CollectionChanged += SearchChanged;
            foreach (SearchBase search in searches)
            {
                wpAdvanced.Children.Add(search.GenerateUI());
            }
            dgMainDataGrid.UnselectAll();
        }

        private void SearchChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            btFilter.IsEnabled = true;
        }

        private readonly ListCollectionView defaultView;
        public Type DataType { get; }
        private readonly InternalObservableCollection<SearchBase> searches;
        private readonly SortingComparer FilterComparer = new SortingComparer();
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
            string conf_str = num_sel == 1 ? $"'{((BaseDataClass)dgMainDataGrid.SelectedItem).Name}'" : $"{num_sel} {DataType.Name.Pluralize()}";
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
            bool visible = gdFilter.Visibility == Visibility.Visible;
            if (visible)
            {
                gdFilter.Visibility = Visibility.Collapsed;
                RefreshFilter();
                return;
            }
            gdFilter.Visibility = Visibility.Visible;
            RefreshFilter();
            filterName.Focus();
        }
        public void RefreshFilter()
        {
            btFilter.IsEnabled = true;
            ListCollectionView view = defaultView;
            string nameFilter = filterName.Text.RemoveWhitespace().ToUpperInvariant();
            string shFilter = filterSh.Text.RemoveWhitespace().ToUpperInvariant();
            dgMainDataGrid.ItemsSource = view;
            if ((string.IsNullOrWhiteSpace(nameFilter) && string.IsNullOrWhiteSpace(shFilter)) || gdFilter.Visibility != Visibility.Visible)
            {
                view.Filter = null;
                view.CustomSort = null;
                return;
            }
            if (cbRemove.IsChecked ?? true)
            {
                Predicate<object> filterPred = DataHelpers.GenerateDefaultNameFilter(nameFilter, shFilter);
                view.Filter = filterPred;
            }
            else
            {
                view.Filter = null;
            }
            if (!string.IsNullOrWhiteSpace(nameFilter))
            {
                FilterComparer.Filter = nameFilter;
                view.CustomSort = FilterComparer;
            }
        }
        public override bool Cancel()
        {
            return true;
        }

        private void FilterClick(object sender, RoutedEventArgs e)
        {
            btFilter.IsEnabled = false;
            string name = filterName.Text.RemoveWhitespace().ToUpperInvariant();
            string sh = filterSh.Text.RemoveWhitespace().ToUpperInvariant();
            bool searchName = !string.IsNullOrWhiteSpace(name);
            FilterComparer.Filter = name;
            IOrderedEnumerable<object> data = defaultView.SourceCollection.Cast<object>().OrderBy(o => searches.Sum(s => s.Search(o) ? 0 : 1));
            if (searchName)
            {
                data = data.ThenBy(o => o, FilterComparer);
            }
            var x = data.ToList();
            ListCollectionView view = new ListCollectionView(x);
            dgMainDataGrid.ItemsSource = view;
            if (cbRemove.IsChecked ?? true)
            {
                Predicate<object> filterPred = DataHelpers.GenerateDefaultNameFilter(name, sh);
                view.Filter = new Predicate<object>(o =>
                {
                    return filterPred(o) && searches.All(s => s.Search(o));
                    });
            }
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
