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
using static TimetablingWPF.DataHelpers;

namespace TimetablingWPF
{
    /// <summary>
    /// Interaction logic for AssignmentTab.xaml
    /// </summary>
    public partial class LessonTab : Grid, ITab
    {
        public LessonTab(Lesson lesson, MainPage mainPage, CommandType commandType)
        {
            InitializeComponent();
            MainPage = mainPage;
            ErrManager = new ErrorManager(spErrors);
            CommandType = commandType;
            OriginalLesson = lesson;
            Lesson = commandType == CommandType.@new ? lesson : (Lesson)lesson.Clone();
            Lesson.Freeze();
            tbTitle.Text = "Create a new Lesson";
            txName.Text = lesson.Name;
            txName.SelectionStart = txName.Text.Length;

            cmbxSubject.ItemsSource = GetData<Subject>();

            HAS_NO_NAME = GenericHelpers.GenerateNameError(ErrManager, txName, "Lesson");
            HAS_NO_SUBJECT = new ErrorContainer(ErrManager, (e) => cmbxSubject.SelectedItem == null, (e) => "No subject has been selected.", ErrorType.Error, false);
        }
        private readonly Lesson Lesson;
        private readonly Lesson OriginalLesson;
        private readonly ErrorContainer HAS_NO_SUBJECT;
        private readonly ErrorContainer HAS_NO_NAME;
        private readonly ErrorManager ErrManager;
        public MainPage MainPage { get; set; }
        private readonly CommandType CommandType;

        private void AssignmentButtonClick(object sender, RoutedEventArgs e)
        {
            Teacher teacher = (Teacher)cmbxAssignmentTeacher.SelectedItem;
            int? lessons = iupdown.Value;
            if (teacher == null || lessons == null)
            {
                return;
            }
            Assignment old = Lesson.Assignments.Where(a => a.Teacher == teacher).SingleOrDefault();
            if (old != null)
            {
                if (old.LessonCount == lessons) { return; }
                RemoveAssignment(old);
            }
            Assignment assignment = new Assignment(teacher, Lesson, (int)lessons);
            AddAssignment(assignment);
            Lesson.Assignments.Add(assignment);
        }

        private void AddAssignment(Assignment assignment)
        {
            spAssignments.Children.Add(VerticalMenuItem(assignment, RemoveAssignmentClick, assignment.LessonString));
        }

        private void RemoveAssignmentClick(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            StackPanel sp = (StackPanel)element.Parent;
            Assignment assignment = (Assignment)element.Tag;
            Lesson.Assignments.Remove(assignment);
            spAssignments.Children.Remove(sp);
        }

        private void RemoveAssignment(Assignment assignment)
        {
            foreach (FrameworkElement sp in spAssignments.Children)
            {
                if ((Assignment)sp.Tag == assignment)
                {
                    spAssignments.Children.Remove(sp);
                    Lesson.Assignments.Remove(assignment);
                    return;
                }
            }
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
            HAS_NO_NAME.UpdateError();
            HAS_NO_SUBJECT.UpdateError();
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
            Lesson.Name = txName.Text;
            Lesson.Unfreeze();
            if (CommandType == CommandType.edit) {
                OriginalLesson.UpdateWithClone(Lesson);
            } else
            {
                Lesson.Commit();
            }
            
            MainPage.CloseTab(this);
        }
    }
}
