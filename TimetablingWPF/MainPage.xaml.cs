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
        public MainPage(IList<Teacher> data)
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
            
            dgTeachers.ItemsSource = data;
        }



        private void ExecuteNewTeacherCommand(object sender, ExecutedRoutedEventArgs e)
        {
            NewTab(new TeacherTab(), "New Teacher");


        }

        private void CanExecuteNewTeacherCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ExecuteEditItemCommand(object sender, ExecutedRoutedEventArgs e)
        {
            //do edit stuff
        }

        private void CanExecuteEditItem(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((DataGrid)e.Parameter).SelectedItems.Count == 1;
        }

        private void ExecuteDuplicateTeacher(object sender, ExecutedRoutedEventArgs e)
        {
            //do duplicate stuff
        }

        private void CanExecuteDuplicateTeacher(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((DataGrid)e.Parameter).SelectedItems.Count == 1;
        }

        private void ExecuteDeleteItem(object sender, ExecutedRoutedEventArgs e)
        {
            
            DataGrid grid = (DataGrid)e.Parameter;
            int num_sel = grid.SelectedItems.Count;
            string teacher_string = num_sel == 1 ? $"'{((BaseDataClass)grid.SelectedItem).Name}'" : $"{num_sel} {grid.Tag}";
            if (MessageBox.Show("Are you sure you want to delete " + teacher_string + "?",
                $"Delete {teacher_string}?", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel)
                == MessageBoxResult.Cancel) { return; }
            foreach (BaseDataClass teacher in grid.SelectedItems)
            {
                teacher.Delete();
                ((IList)Application.Current.Properties[(string)grid.Tag]).Remove(teacher);
            }
        }

        private void CanExecuteDeleteItem(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((DataGrid)e.Parameter).SelectedItems.Count >= 1;
        }

        public void NewTab(object page, string title, bool focus=true)
        {
            TabItem newTab = new TabItem()
            {
                Content = new Frame() { Content = page },
                Header = title
            };
            tcMainTabControl.Items.Add(newTab);
            if (focus) { tcMainTabControl.SelectedItem = newTab; }
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

    public class ListFormatter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return "N/A";
            }
            IEnumerable<BaseDataClass> enumerable = ((IList)value).Cast<BaseDataClass>();
            return String.Join<BaseDataClass>(", ", enumerable);
        }
        public object ConvertBack(object value, Type targetType, object paramter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
