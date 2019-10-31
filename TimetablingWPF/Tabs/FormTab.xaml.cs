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
    public partial class FormTab : Grid, ITab
    {
        public FormTab(Form form, MainPage mainPage, CommandType commandType)
        {
            InitializeComponent();
            MainPage = mainPage;
            ErrManager = new ErrorManager(spErrors);
            CommandType = commandType;
            OriginalSet = form ?? throw new ArgumentNullException(nameof(form));
            Form = commandType == CommandType.@new ? form : (Form)form.Clone();
            tbTitle.Text = "Create a new Form";
            txName.Text = form.Name;
            txName.SelectionStart = txName.Text.Length;
            cmbxSubject.ItemsSource = (IEnumerable<Subject>)Application.Current.Properties[Subject.ListName];
            cmbxAssignmentSubject.ItemsSource = (IEnumerable<Subject>)Application.Current.Properties[Subject.ListName];
            cmbxAssignmentSubject.comboBox.SelectionChanged += CmbxAssignmentsSubjectSelectionChanged;
            cmbxAssignmentTeacher.ItemsSource = (IEnumerable<Teacher>)Application.Current.Properties[Teacher.ListName];
            //Errors
        }

        private void SubjectButtonClick(object sender, RoutedEventArgs e)
        {
            
            Subject subject = (Subject)cmbxSubject.SelectedItem;
            if (subject == null)
            {
                if (string.IsNullOrWhiteSpace(cmbxSubject.Text))
                {
                    return;
                }
                subject = new Subject() { Name = cmbxSubject.Text.Trim() };
                subject.Commit();
            }
            AddSubject(subject);
            cmbxSubject.SelectedItem = subject;
        }

        private void AssignmentButtonClick(object sender, RoutedEventArgs e)
        {
            Teacher teacher = (Teacher)cmbxAssignmentTeacher.SelectedItem;
            int? periods = iupdownAssignment.Value;
            if (teacher == null || periods == null)
            {
                return;
            }
            Assignment assignment = new Assignment(teacher, (int)periods, (Subject)cmbxAssignmentSubject.SelectedItem);
            AddAssignment(assignment);
            Form.Assignments.Add(assignment);
        }

        private void AddSubject(Subject subject)
        {            
            spSubject.Children.Add(VerticalMenuItem(subject, RemoveSubject));
        }

        private void RemoveSubject(object sender, RoutedEventArgs e)
        {
            StackPanel sp = (StackPanel)((FrameworkElement)sender).Tag;
            Subject subject = (Subject)sp.Tag;
            spSubject.Children.Remove(sp);
        }

        private void AddAssignment(Assignment assignment)
        {
            spAssignments.Children.Add(VerticalMenuItem(assignment, RemoveAssignment));
        }

        private void RemoveAssignment(object sender, RoutedEventArgs e)
        {
            StackPanel sp = (StackPanel)((FrameworkElement)sender).Tag;
            Assignment assignment = (Assignment)sp.Tag;
            Form.Assignments.Remove(assignment);
            spAssignments.Children.Remove(sp);
        }
        private readonly Form Form;
        private readonly Form OriginalSet;
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
            Form.Name = txName.Text;

            if (CommandType == CommandType.edit) {
                OriginalSet.Recommit(Form);
            } else
            {
                Form.Commit();
            }
            
            MainPage.CloseTab(this);
        }
    }
}
