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
    /// Interaction logic for AssignmentTab.xaml
    /// </summary>
    public partial class ClassTab : Grid, ITab
    {
        public ClassTab(Class @class, MainPage mainPage, CommandType commandType)
        {
            InitializeComponent();
            MainPage = mainPage;
            ErrManager = new ErrorManager(spErrors);
            CommandType = commandType;
            OriginalClass = @class ?? throw new ArgumentNullException(nameof(@class));
            Class = commandType == CommandType.@new ? @class : (Class)@class.Clone();
            tbTitle.Text = "Create a new Class";
            txName.Text = @class.Name;
            txName.SelectionStart = txName.Text.Length;
            cmbxGroups.ItemsSource = (IEnumerable<Group>)Application.Current.Properties[Group.ListName];
            cmbxSubject.ItemsSource = (IEnumerable<Subject>)Application.Current.Properties[Subject.ListName];
            cmbxAssignmentSubject.ItemsSource = (IEnumerable<Subject>)Application.Current.Properties[Subject.ListName];
            cmbxAssignmentSubject.comboBox.SelectionChanged += CmbxAssignmentsSubjectSelectionChanged;
            cmbxAssignmentTeacher.ItemsSource = (IEnumerable<Teacher>)Application.Current.Properties[Teacher.ListName];
            //Errors
        }

        private void GroupButtonClick(object sender, RoutedEventArgs e)
        {
            
            Group group = (Group)cmbxGroups.SelectedItem;
            if (group == null)
            {
                if (string.IsNullOrWhiteSpace(cmbxGroups.Text))
                {
                    return;
                }
                group = new Group() { Name = cmbxGroups.Text.Trim() };
                group.Commit();
            }
            AddGroup(group);
            cmbxGroups.SelectedItem = group;
        }

        private void AssignmentButtonClick(object sender, RoutedEventArgs e)
        {
            Teacher teacher = (Teacher)cmbxAssignmentTeacher.SelectedItem;
            int? periods = iupdownAssignment.Value;
            if (teacher == null || periods == null)
            {
                return;
            }
            Assignment assignment = new Assignment(teacher, (int)periods);
            AddAssignment(assignment);
            Class.Assignments.Add(assignment);
        }

        private void AddGroup(Group group)
        {            
            spGroups.Children.Add(VerticalMenuItem(group, RemoveGroup));
        }

        private void RemoveGroup(object sender, RoutedEventArgs e)
        {
            StackPanel sp = (StackPanel)((FrameworkElement)sender).Tag;
            Group group = (Group)sp.Tag;
            Class.Groups.Remove(group);
            spGroups.Children.Remove(sp);
        }

        private void AddAssignment(Assignment assignment)
        {
            spAssignments.Children.Add(VerticalMenuItem(assignment, RemoveAssignment));
        }

        private void RemoveAssignment(object sender, RoutedEventArgs e)
        {
            StackPanel sp = (StackPanel)((FrameworkElement)sender).Tag;
            Assignment assignment = (Assignment)sp.Tag;
            Class.Assignments.Remove(assignment);
            spAssignments.Children.Remove(sp);
        }
        private readonly Class Class;
        private readonly Class OriginalClass;
        private readonly Error HAS_EMPTY_NAME = new Error("Assignment does not have a name", ErrorType.Error);
        private readonly ErrorManager ErrManager;
        public MainPage MainPage { get; set; }
        private readonly CommandType CommandType;

        private void CmbxAssignmentsSubjectSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmbx = (ComboBox)sender;
            Subject subject = (Subject)cmbxAssignmentSubject.SelectedItem;
            IEnumerable<Teacher> all_teachers = (IEnumerable<Teacher>)Application.Current.Properties[Teacher.ListName];
            if (subject == null)
            {
                cmbxAssignmentTeacher.ItemsSource = all_teachers;
                return;
            }
            IEnumerable<Teacher> teachers = from teacher in all_teachers where teacher.Subjects.Contains(subject) select teacher;
            cmbxAssignmentTeacher.ItemsSource = teachers;
        }

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
            Class.Name = txName.Text;

            if (CommandType == CommandType.edit) {
                OriginalClass.Recommit(Class);
            } else
            {
                Class.Commit();
            }
            
            MainPage.CloseTab(this);
        }
    }
}
