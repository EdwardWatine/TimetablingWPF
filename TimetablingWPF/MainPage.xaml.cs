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
    public partial class MainPage : Page
    {
        public MainPage()
        {
            Application.Current.MainWindow.WindowState = WindowState.Maximized;
            Application.Current.MainWindow.ResizeMode = ResizeMode.CanResize;
            Application.Current.MainWindow.SizeToContent = SizeToContent.Manual;
            InitializeComponent();
            void attachCommand(string key, ICommand command, object parameter = null)
            {
                ((MenuItem)Resources[key]).CommandParameter = parameter;
                ((MenuItem)Resources[key]).Command = command;
            }
            attachCommand("miEditTeacher", Commands.EditItem, dgTeachers);
            attachCommand("miNewTeacher", Commands.NewTeacher);
            attachCommand("miDeleteTeachers", Commands.DeleteItem, dgTeachers);
            attachCommand("miDuplicateTeacher", Commands.DuplicateTeacher, dgTeachers);

            dgTeachers.ItemsSource = (IList)Application.Current.Properties["Teachers"];
        }



        private void ExecuteNewTeacherCommand(object sender, ExecutedRoutedEventArgs e)
        {
            NewTab(new TeacherTab(new Teacher(), CommandType.@new), "New Teacher");
        }

        private void CanExecuteNewTeacherCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ExecuteEditItemCommand(object sender, ExecutedRoutedEventArgs e)
        {
            switch (((DataGrid)e.Parameter).SelectedItem)
            {
                case Teacher teacher:
                    NewTab(new TeacherTab(teacher, CommandType.edit), "Edit Teacher");
                    break;
            }
        }

        private void CanExecuteEditItem(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((DataGrid)e.Parameter).SelectedItems.Count == 1;
        }

        private void ExecuteDuplicateTeacher(object sender, ExecutedRoutedEventArgs e)
        {
            Teacher teacher = (Teacher)((DataGrid)e.Parameter).SelectedItem;
            NewTab(new TeacherTab(teacher, CommandType.copy), "New Teacher");
        }

        private void CanExecuteDuplicateTeacher(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((DataGrid)e.Parameter).SelectedItems.Count == 1;
        }

        private void ExecuteDeleteItem(object sender, ExecutedRoutedEventArgs e)
        {
            DataGrid grid = (DataGrid)e.Parameter;
            int num_sel = grid.SelectedItems.Count;
            string conf_str = num_sel == 1 ? $"'{((BaseDataClass)grid.SelectedItem).Name}'" : $"{num_sel} {grid.Tag}";
            if (MessageBox.Show("Are you sure you want to delete " + conf_str + "?",
                $"Delete {conf_str}?", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel)
                == MessageBoxResult.Cancel) { return; }
            for (int i = 0; i < grid.SelectedItems.Count;)
            {
                ((BaseDataClass)grid.SelectedItems[i]).Delete();
            }
        }

        private void CanExecuteDeleteItem(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((DataGrid)e.Parameter).SelectedItems.Count >= 1;
        }

        public void NewTab(object page, string title, bool focus = true)
        {
            TabItem newTab = new TabItem()
            {
                Content = new Frame() { Content = page },
                Header = title
            };
            tcMainTabControl.Items.Add(newTab);
            if (focus) { tcMainTabControl.SelectedItem = newTab; }
        }

        public void CloseTab(object page)
        {
            foreach (TabItem tab in tcMainTabControl.Items)
            {
                if ((tab.Content as Frame)?.Content == page)
                {
                    tcMainTabControl.Items.Remove(tab);
                    return;
                }
            }
            throw new System.ArgumentException($"Page {page} of type {page.GetType().Name} does not exist in the tab list");
        }
    }

    public class Commands
    {
        public static readonly RoutedUICommand NewTeacher = new RoutedUICommand(
            "NewTeacher", "NewTeacher", typeof(Commands));
        public static readonly RoutedUICommand EditItem = new RoutedUICommand(
            "EditItem", "EditItem", typeof(Commands));
        public static readonly RoutedUICommand DuplicateTeacher = new RoutedUICommand(
            "DuplicateTeacher", "DuplicateTeacher", typeof(Commands));
        public static readonly RoutedUICommand DeleteItem = new RoutedUICommand(
            "DeleteItem", "DeleteItem", typeof(Commands));
    }

    public enum CommandType : byte {
        @new,
        edit,
        copy
    }

    public class ListFormatter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return "N/A";
            }
            if (((IList)value).Count == 0)
            {
                return "None";
            }
            IEnumerable<object> enumerable = ((IList)value).Cast<object>();
            return string.Join(", ", enumerable);
        }
        public object ConvertBack(object value, Type targetType, object paramter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
