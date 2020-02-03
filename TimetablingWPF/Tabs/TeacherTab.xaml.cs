using Humanizer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
using static TimetablingWPF.GenericHelpers;
using static TimetablingWPF.DataHelpers;

namespace TimetablingWPF
{
    /// <summary>
    /// Interaction logic for TeacherTab.xaml
    /// </summary>
    public partial class TeacherTab : TabBase
    {
        public TeacherTab(Teacher teacher, CommandType commandType)
        {
            InitializeComponent();

            ErrManager = new ErrorManager(spErrors);
            CommandType = commandType;
            OriginalTeacher = teacher;
            Teacher = commandType == CommandType.@new ? teacher : (Teacher)teacher.Clone();
            Teacher.Freeze();
            tbTitle.Text = "Create a new Teacher";
            txName.Text = teacher.Name;
            txName.SelectionStart = txName.Text.Length;
            cmbxSubjects.ItemsSource = GetDataContainer().Subjects;
            cmbxAssignmentLesson.ItemsSource = GetDataContainer().Lessons;
            iupdownMax.Value = Teacher.MaxPeriodsPerCycle;

            HAS_NO_NAME = GenerateNameError(ErrManager, txName, "Teacher");

            HAS_NO_PERIODS = new ErrorContainer(ErrManager, (e) => Teacher.UnavailablePeriods.Count == TimetableStructure.TotalSchedulable,
                (e) => "Teacher has no free periods.", ErrorType.Warning);
            HAS_NO_PERIODS.BindCollection(Teacher.UnavailablePeriods);

            NOT_ENOUGH_PERIODS = new ErrorContainer(ErrManager,
                (e) => TimetableStructure.TotalSchedulable - Teacher.UnavailablePeriods.Count < Teacher.Assignments.Sum(x => x.LessonCount),
                (e) => $"Teacher has fewer free periods ({TimetableStructure.TotalSchedulable - Teacher.UnavailablePeriods.Count}) than assigned periods " +
                $"({Teacher.Assignments.Sum(x => x.LessonCount)}).", ErrorType.Error);
            NOT_ENOUGH_PERIODS.BindCollection(Teacher.UnavailablePeriods);
            NOT_ENOUGH_PERIODS.BindCollection(Teacher.Assignments);

            SUBJECT_NO_ASSIGNMENT = new ErrorContainer(ErrManager,
                (e) =>
                {
                    IEnumerable<Subject> subjectMismatches = Teacher.Subjects.Except(Teacher.Assignments.Select(a => a.Lesson.Subject));
                    e.Data = subjectMismatches;
                    return subjectMismatches.Any();
                },
                (e) =>
                {
                    IEnumerable<Subject> data = (IEnumerable<Subject>)e.Data;
                    return $"The following Subjects have no assignments: {FormatEnumerable(data)}.";
                }, ErrorType.Warning);
            SUBJECT_NO_ASSIGNMENT.BindCollection(Teacher.Assignments);
            SUBJECT_NO_ASSIGNMENT.BindCollection(Teacher.Subjects);

            ASSIGNMENT_NO_SUBJECT = new ErrorContainer(ErrManager,
                (e) =>
                {
                    IEnumerable<Assignment> assignmentMismatches = Teacher.Assignments.Where(a => !Teacher.Subjects.Contains(a.Lesson.Subject));
                    e.Data = assignmentMismatches;
                    return assignmentMismatches.Any();
                },
                (e) =>
                {
                    IEnumerable<Assignment> data = (IEnumerable<Assignment>)e.Data;
                    return $"The following Assignments have a subject that the teacher does not have: {FormatEnumerable(data)}.";
                }, ErrorType.Warning);
            ASSIGNMENT_NO_SUBJECT.BindCollection(Teacher.Assignments);
            ASSIGNMENT_NO_SUBJECT.BindCollection(Teacher.Subjects);

            NOT_ENOUGH_FORM_SLOTS = new ErrorContainer(ErrManager,
                (e) =>
                {
                    IEnumerable<Lesson> errors = Teacher.Assignments.Where(a => a.Lesson.LessonsPerCycle < a.Lesson.Assignments.Sum(a2 => a.LessonCount)).Select(a => a.Lesson);
                    e.Data = errors;
                    return errors.Any();
                },
                (e) =>
                {
                    IEnumerable<Lesson> errors = (IEnumerable<Lesson>)e.Data;
                    return $"The following lessons have more assignments than have been allocated to it: {FormatEnumerable(errors)}.";
                }, ErrorType.Error);
            NOT_ENOUGH_FORM_SLOTS.BindCollection(Teacher.Assignments);

            foreach (Assignment assignment in Teacher.Assignments)
            {
                AddAssignment(assignment);
            }

            svPeriods.Content = GenerateTimetable(Teacher.UnavailablePeriods, ToggleSlot, ToggleAll);
            ilSubjects.ItemsSource = Teacher.Subjects;
            ilSubjects.ListenToCollection(OriginalTeacher.Subjects);
        }

        private void SubjectButtonClick(object sender, RoutedEventArgs e)
        {

            Subject subject = cmbxSubjects.GetObject<Subject>();
            if (subject != null)
            {
                subject.Commit();
                cmbxSubjects.SelectedItem = subject;
                Teacher.Subjects.Add(subject);
            }
        }

        private void AssignmentButtonClick(object sender, RoutedEventArgs e)
        {
            Lesson lesson = (Lesson)cmbxAssignmentLesson.SelectedItem;
            int? lessons = iupdown.Value;
            if (lesson == null || lessons == null)
            {
                return;
            }
            Assignment old = Teacher.Assignments.Where(a => a.Lesson == lesson).SingleOrDefault();
            if (old != null)
            {
                if (old.LessonCount == lessons) { return; }
                RemoveAssignment(old);
            }
            Assignment assignment = new Assignment(Teacher, lesson, (int)lessons);
            AddAssignment(assignment);
            Teacher.Assignments.Add(assignment);
        }

        private void AddAssignment(Assignment assignment)
        {
            spAssignments.Children.Add(VerticalMenuItem(assignment, RemoveAssignmentClick, assignment.TeacherString));
        }

        private void RemoveAssignmentClick(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            StackPanel sp = (StackPanel)element.Parent;
            Assignment assignment = (Assignment)element.Tag;
            Teacher.Assignments.Remove(assignment);
            spAssignments.Children.Remove(sp);
        }

        private void RemoveAssignment(Assignment assignment)
        {
            foreach (FrameworkElement sp in spAssignments.Children)
            {
                if ((Assignment)sp.Tag == assignment)
                {
                    spAssignments.Children.Remove(sp);
                    Teacher.Assignments.Remove(assignment);
                    return;
                }
            }
        }

        private readonly Teacher Teacher;
        private readonly Teacher OriginalTeacher;
        private readonly ErrorContainer HAS_NO_PERIODS;
        private readonly ErrorContainer NOT_ENOUGH_PERIODS;
        private readonly ErrorContainer SUBJECT_NO_ASSIGNMENT;
        private readonly ErrorContainer ASSIGNMENT_NO_SUBJECT;
        private readonly ErrorContainer NOT_ENOUGH_FORM_SLOTS;
        private readonly ErrorContainer HAS_NO_NAME;
        private readonly ErrorManager ErrManager;
        
        private readonly CommandType CommandType;

        private void ToggleAll(object sender, MouseButtonEventArgs e)
        {
            Grid grid = (Grid)sender;
            foreach (Border border in grid.Children)
            {
                if (border.Child is Rectangle rect && rect.Tag != null)
                {
                    ToggleSlot(rect, null);
                }
            }

        }

        private void ToggleSlot(object sender, MouseButtonEventArgs e)
        {
            Rectangle rect = (Rectangle)sender;
            Tuple<TimetableSlot, bool> tag = (Tuple<TimetableSlot, bool>)rect.Tag;
            rect.Tag = Tuple.Create(tag.Item1, !tag.Item2);
            rect.Fill = (SolidColorBrush)new BrushConverter().ConvertFromString(tag.Item2 ? "#FF0000" : "#00FF00");
            if (tag.Item2)
            {
                Teacher.UnavailablePeriods.Add(tag.Item1);
            }
            else
            {
                Teacher.UnavailablePeriods.Remove(tag.Item1);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (Cancel())
            {
                MainPage.CloseTab(this);
            }
        }

        private void Confirm(object sender, RoutedEventArgs e)
        {
            HAS_NO_NAME.UpdateError();
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
            Teacher.Name = txName.Text;
            Teacher.MaxPeriodsPerCycle = iupdownMax.Value ?? Teacher.MaxPeriodsPerCycle;
            Teacher.Unfreeze();
            if (CommandType == CommandType.edit) {
                OriginalTeacher.UpdateWithClone(Teacher);
            } else
            {
                Teacher.Commit();
            }
            MainPage.CloseTab(this);
        }
    }
}
