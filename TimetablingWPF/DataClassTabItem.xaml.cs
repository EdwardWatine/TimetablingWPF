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
    public partial class DataSetTabItem : TabItem, ITab
    {
        public DataSetTabItem(MainPage mainPage, Type type)
        {
            InitializeComponent();
            MainPage = mainPage;
            DataType = type;
            void attachCommand(string key, ICommand command, object parameter = null)
            {
                ((MenuItem)Resources[key]).CommandParameter = parameter;
                ((MenuItem)Resources[key]).Command = command;
            }
            attachCommand($"miEditItem", Commands.EditItem, dgMainDataGrid);
            attachCommand($"miNewItem", Commands.NewItem, type);
            attachCommand($"miDeleteItem", Commands.DeleteItem, dgMainDataGrid);
            attachCommand($"miDuplicateItem", Commands.DuplicateItem, dgMainDataGrid);

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
        private void ExecuteNewItemCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Type type = (Type)e.Parameter;
            MainPage.NewTab(
                Activator.CreateInstance(TypeTab[type], new object[] { Activator.CreateInstance(type), MainPage, CommandType.@new }),
                $"New {type.Name}");
        }

        private void CanExecuteNewItemCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ExecuteEditItemCommand(object sender, ExecutedRoutedEventArgs e)
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
            object item = (((DataGrid)e.Parameter).SelectedItem);
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
    public static class Commands
    {
        public static readonly RoutedUICommand NewItem = new RoutedUICommand(
            "NewTeacher", "NewTeacher", typeof(Commands));
        public static readonly RoutedUICommand EditItem = new RoutedUICommand(
            "EditItem", "EditItem", typeof(Commands));
        public static readonly RoutedUICommand DuplicateItem = new RoutedUICommand(
            "DuplicateItem", "DuplicateItem", typeof(Commands));
        public static readonly RoutedUICommand DeleteItem = new RoutedUICommand(
            "DeleteItem", "DeleteItem", typeof(Commands));
    }

    public enum CommandType
    {
        @new,
        edit,
        copy
    }
}
