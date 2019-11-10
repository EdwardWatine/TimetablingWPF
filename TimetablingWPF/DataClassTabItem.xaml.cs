using System;
using System.Collections;
using System.Collections.Generic;
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

namespace TimetablingWPF
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class DataClassTabItem : TabItem, ITab
    {
        public DataClassTabItem(MainPage mainPage, Type type)
        {
            InitializeComponent();
            MainPage = mainPage;
            DataType = type;
            void attachCommand(string key, ICommand command,
                ExecutedRoutedEventHandler executed,
                CanExecuteRoutedEventHandler canExecute,
                object parameter = null)
            {
                ((MenuItem)Resources[key]).CommandParameter = parameter;
                ((MenuItem)Resources[key]).CommandBindings.Add(new CommandBinding(command, executed, canExecute));
                ((MenuItem)Resources[key]).Command = command;
            }
            attachCommand($"miEditItem", DataGridCommands.EditItem, ExecuteEditItem, CanExecuteEditItem, dgMainDataGrid);
            attachCommand($"miNewItem", DataGridCommands.NewItem, ExecuteNewItem, CanExecuteNewItem, type);
            attachCommand($"miDeleteItem", DataGridCommands.DeleteItem, ExecuteDeleteItem, CanExecuteDeleteItem, dgMainDataGrid);
            attachCommand($"miDuplicateItem", DataGridCommands.DuplicateItem, ExecuteDuplicateItem, CanExecuteDuplicateItem, dgMainDataGrid);

            dgMainDataGrid.ItemsSource = (IList)Application.Current.Properties[type];
            dgMainDataGrid.Columns.Add(new DataGridTemplateColumn()
            {
                CanUserSort = true,
                SortMemberPath = "Name",
                Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                MinWidth = 20,
                Header = "Name",
                CellTemplate = (DataTemplate)Resources["NameTemplate"]
            }); ;
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
        }
        public MainPage MainPage { get; set; }
        public Type DataType { get; }
        private readonly Dictionary<Type, Type> TypeTab = new Dictionary<Type, Type>()
        {
            {typeof(Subject), typeof(SubjectTab) },
            {typeof(Teacher), typeof(TeacherTab) },
            {typeof(Lesson), typeof(LessonTab) },
            {typeof(Group), typeof(GroupTab) },
            {typeof(Form), typeof(FormTab) },
            {typeof(Room), typeof(RoomTab) }

        };
        private readonly Dictionary<string, string[]> Columns = new Dictionary<string, string[]>()
            {
                { "Teacher", new string[]{"Subjects", "Assignments", "Unavailable Periods" } },
                { "Subject", new string[]{"Teachers", "Groups" } },
                { "Lesson", new string[]{"Subject", "Lessons Per Cycle", "Lesson Length", "Assignments" } },
                { "Group", new string[]{"Subjects", "Rooms" } },
                { "Form", new string[]{"Year Group", "Lessons" } },
                { "Room", new string[]{"Quantity", "Critical", "Groups"} }
            };
        private readonly HashSet<string> Shortcols = new HashSet<string>() { "Lessons Per Cycle", "Lesson Length", "Quantity", "Critical", "Subject", "Year Group" };
        private void ExecuteNewItem(object sender, ExecutedRoutedEventArgs e)
        {
            Type type = (Type)e.Parameter;
            MainPage.NewTab(
                Activator.CreateInstance(TypeTab[type], new object[] { Activator.CreateInstance(type), MainPage, CommandType.@new }),
                $"New {type.Name}");
        }

        private void CanExecuteNewItem(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ExecuteEditItem(object sender, ExecutedRoutedEventArgs e)
        {
            object item = (((DataGrid)e.Parameter).SelectedItem);
            Type type = item.GetType();
            MainPage.NewTab(
                Activator.CreateInstance(TypeTab[type], new object[] { item, MainPage, CommandType.edit }),
                $"Edit {type.Name}");

        }

        private void CanExecuteEditItem(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((DataGrid)e.Parameter).SelectedItems.Count == 1;
        }

        private void ExecuteDuplicateItem(object sender, ExecutedRoutedEventArgs e)
        {
            object item = ((DataGrid)e.Parameter).SelectedItem;
            Type type = item.GetType();
            MainPage.NewTab(
                Activator.CreateInstance(TypeTab[type], new object[] { item, MainPage, CommandType.edit }),
                $"Edit {type.Name}");
        }

        private void CanExecuteDuplicateItem(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((DataGrid)e.Parameter).SelectedItems.Count == 1;
        }

        private void ExecuteDeleteItem(object sender, ExecutedRoutedEventArgs e)
        {
            DataGrid grid = (DataGrid)e.Parameter;
            int num_sel = grid.SelectedItems.Count;
            string conf_str = num_sel == 1 ? $"'{((BaseDataClass)grid.SelectedItem).Name}'" : $"{num_sel} {grid.Tag}";
            if (VisualHelpers.ShowWarningBox("Are you sure you want to delete " + conf_str + "?", $"Delete {conf_str}?") == MessageBoxResult.Cancel) { return; }
            for (int i = 0; i < grid.SelectedItems.Count; i++)
            {
                ((BaseDataClass)grid.SelectedItems[i]).Delete();
            }
        }

        private void CanExecuteDeleteItem(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((DataGrid)e.Parameter).SelectedItems.Count >= 1;
        }

        public void Cancel()
        {
            return;
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
    }

    public enum CommandType
    {
        @new,
        edit,
        copy
    }
}
