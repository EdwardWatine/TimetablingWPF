using Humanizer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;
using static TimetablingWPF.VisualHelpers;

namespace TimetablingWPF
{
    /// <summary>
    /// Interaction logic for TeacherTab.xaml
    /// </summary>
    public partial class SubjectTab : Grid, ITab
    {
        public SubjectTab(Subject subject, MainPage mainPage, CommandType commandType)
        {
            InitializeComponent();
            MainPage = mainPage;
            ErrManager = new ErrorManager(spErrors);
            CommandType = commandType;
            OriginalSubject = subject;
            Subject = commandType == CommandType.@new ? subject : (Subject)subject.Clone();
            tbTitle.Text = "Create a new Subject";
            txName.Text = subject.Name;
            txName.SelectionStart = txName.Text.Length;
            cmbxGroups.ItemsSource = (IEnumerable<Group>)Application.Current.Properties[Group.ListName];
            cmbxTeachers.ItemsSource = (IEnumerable<Teacher>)Application.Current.Properties[Teacher.ListName];
            //Errors

            foreach (Group group in Subject.Groups)
            {
                AddGroup(group);
            }
            foreach (Teacher teacher in Subject.Teachers)
            {
                AddTeacher(teacher);
            }
        }

        private void GroupButtonClick(object sender, RoutedEventArgs e)
        {
            
            Group group = (Group)cmbxGroups.SelectedItem;
            if (group == null)
            {
                return;
            }
            AddGroup(group);
            cmbxGroups.SelectedItem = group;
            Subject.Groups.Add(group);
        }

        private void TeacherButtonClick(object sender, RoutedEventArgs e)
        {
            Teacher teacher = (Teacher)cmbxTeachers.SelectedItem;
            if (teacher == null)
            {
                return;
            }
            AddTeacher(teacher);
            Subject.Teachers.Add(teacher);
        }

        private void AddGroup(Group group)
        {            
            spGroups.Children.Add(VerticalMenuItem(group, RemoveGroup));
        }

        private void RemoveGroup(object sender, RoutedEventArgs e)
        {
            StackPanel sp = (StackPanel)((FrameworkElement)sender).Tag;
            Group group = (Group)sp.Tag;
            Subject.Groups.Remove(group);
            spGroups.Children.Remove(sp);
        }

        private void AddTeacher(Teacher teacher)
        {
            spTeachers.Children.Add(VerticalMenuItem(teacher, RemoveTeacher));
        }

        private void RemoveTeacher(object sender, RoutedEventArgs e)
        {
            StackPanel sp = (StackPanel)((FrameworkElement)sender).Tag;
            Teacher teacher = (Teacher)sp.Tag;
            Subject.Teachers.Remove(teacher);
            spTeachers.Children.Remove(sp);
        }
        private readonly Subject Subject;
        private readonly Subject OriginalSubject;
        private readonly Error HAS_EMPTY_NAME = new Error("Teacher does not have a name", ErrorType.Error);
        private readonly ErrorManager ErrManager;
        public MainPage MainPage { get; set; }
        private CommandType CommandType;

        private void TxNameChanged(object sender, TextChangedEventArgs e)
        {
            ErrManager.UpdateError(HAS_EMPTY_NAME, string.IsNullOrWhiteSpace(txName.Text));
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Cancel();
        }

        public void Cancel()
        {
            if (System.Windows.MessageBox.Show("Are you sure you want to discard your changes?",
                "Discard changes?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                MainPage.CloseTab(this);
            }
        }

        private void Confirm(object sender, RoutedEventArgs e)
        {
            ErrManager.UpdateError(HAS_EMPTY_NAME, string.IsNullOrWhiteSpace(txName.Text));
            if (ErrManager.GetNumErrors() > 0)
            {
                ShowErrorBox("Please fix all errors!");
                return;
            }
            if (ErrManager.GetNumWarnings() > 0)
            {
                if (System.Windows.MessageBox.Show("There are warnings. Do you want to continue?", "Warning", 
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                {
                    return;
                }
            }
            Subject.Name = txName.Text;

            if (CommandType == CommandType.edit) {
                OriginalSubject.Recommit(Subject);
            } else
            {
                Subject.Commit();
            }
            
            MainPage.CloseTab(this);
        }
    }
}
