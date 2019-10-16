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
            void attachCommand(string key, ICommand command, object parameter = null)
            {
                ((MenuItem)Resources[key]).CommandParameter = parameter;
                ((MenuItem)Resources[key]).Command = command;
            }
            attachCommand($"miEditItem", Commands.EditItem, dgMainDataGrid);
            attachCommand($"miNewItem", Commands.NewItem, type);
            attachCommand($"miDeleteItem", Commands.DeleteItem, dgMainDataGrid);
            attachCommand($"miDuplicateItem", Commands.DuplicateItem, dgMainDataGrid);

            dgMainDataGrid.ItemsSource = (IList)Application.Current.Properties[type.GetField("ListName").GetValue(type)];
            Dictionary<string, string[]> columns = new Dictionary<string, string[]>()
            {
                { "Teacher", new string[]{"Subjects", "Assignments", "Unavailable Periods" } },
                { "Subject", new string[]{"Teachers", "Rooms" } }
            };
            dgMainDataGrid.Columns.Add(new DataGridTemplateColumn()
            {
                CanUserSort = true,
                SortMemberPath = "Name",
                Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                Header = "Name",
                CellTemplate = (DataTemplate)Resources["NameTemplate"]
            });
            int width = columns[type.Name].Length;
            foreach (string column in columns[type.Name])
            {
                dgMainDataGrid.Columns.Add(new DataGridTemplateColumn()
                {
                    Width = new DataGridLength(width, DataGridLengthUnitType.Star),
                    Header = column,
                    CellTemplate = (DataTemplate)Resources[$"{column}Template"]
                });
            }
        }
        public MainPage MainPage { get; set; }
        public readonly Type DataType;
        private void ExecuteNewItemCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Type type = (Type)e.Parameter;
            if (type == typeof(Teacher))
            {
                MainPage.NewTab(new TeacherTab(new Teacher(), MainPage, CommandType.@new), "New Teacher");
                return;
            }
            if (type == typeof(Subject))
            {
                MainPage.NewTab(new SubjectTab(new Subject(), MainPage, CommandType.@new), "New Subject");
            }
        }

        private void CanExecuteNewItemCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ExecuteEditItemCommand(object sender, ExecutedRoutedEventArgs e)
        {
            switch (((DataGrid)e.Parameter).SelectedItem)
            {
                case Teacher teacher:
                    MainPage.NewTab(new TeacherTab(teacher, MainPage, CommandType.edit), "Edit Teacher");
                    break;
                case Subject subject:
                    MainPage.NewTab(new SubjectTab(subject, MainPage, CommandType.edit), "Edit Subject");
                    break;
            }
        }

        private void CanExecuteEditItem(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((DataGrid)e.Parameter).SelectedItems.Count == 1;
        }

        private void ExecuteDuplicateItem(object sender, ExecutedRoutedEventArgs e)
        {
            switch (((DataGrid)e.Parameter).SelectedItem)
            {
                case Teacher teacher:
                    MainPage.NewTab(new TeacherTab(teacher, MainPage, CommandType.copy), "New Teacher");
                    break;
                case Subject subject:
                    MainPage.NewTab(new SubjectTab(subject, MainPage, CommandType.copy), "New Subject");
                    break;
            }

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
    public class Commands
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

    public enum CommandType : byte
    {
        @new,
        edit,
        copy
    }
}
