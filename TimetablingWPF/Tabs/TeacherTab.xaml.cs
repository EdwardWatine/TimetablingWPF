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

namespace TimetablingWPF
{
    /// <summary>
    /// Interaction logic for TeacherTab.xaml
    /// </summary>
    public partial class TeacherTab : Grid, ITab
    {
        public TeacherTab(Teacher teacher, MainPage mainPage, CommandType commandType)
        {
            InitializeComponent();
            MainPage = mainPage;
            ErrManager = new ErrorManager(spErrors);
            CommandType = commandType;
            OriginalTeacher = teacher;
            Teacher = (Teacher)teacher.Clone();
            tbTitle.Text = "Create a new Teacher";
            txName.Text = teacher.Name;
            txName.SelectionStart = txName.Text.Length;
            cmbxSubjects.ItemsSource = (IEnumerable<Subject>)Application.Current.Properties[Subject.ListName];
            cmbxAssignmentSubject.ItemsSource = cmbxSubjects.ItemsSource;
            cmbxAssignmentForm.ItemsSource = (IEnumerable<Form>)Application.Current.Properties[Form.ListName];
            cmbxAssignmentSubject.comboBox.SelectionChanged += CmbxAssignmentsSubjectsSelectionChanged;

            HAS_EMPTY_NAME = new Error(ErrManager, (e) => string.IsNullOrWhiteSpace(txName.Text),
                (e) => "Teacher has no name.", ErrorType.Error, false);
            txName.TextChanged += delegate (object sender, TextChangedEventArgs e) { HAS_EMPTY_NAME.UpdateError(); };

            HAS_NO_PERIODS = new Error(ErrManager, (e) => Teacher.UnavailablePeriods.Count == Structure.TotalFreePeriods,
                (e) => "Teacher has no free periods.", ErrorType.Warning);
            HAS_NO_PERIODS.BindCollection(Teacher.UnavailablePeriods);

            NOT_ENOUGH_PERIODS = new Error(ErrManager,
                (e) => Structure.TotalFreePeriods - Teacher.UnavailablePeriods.Count < Teacher.Assignments.Sum(x => x.LessonCount),
                (e) => $"Teacher has fewer free periods ({Structure.TotalFreePeriods - Teacher.UnavailablePeriods.Count}) than assigned periods " +
                $"({Teacher.Assignments.Sum(x => x.LessonCount)}).", ErrorType.Error);
            NOT_ENOUGH_PERIODS.BindCollection(Teacher.UnavailablePeriods);
            NOT_ENOUGH_PERIODS.BindCollection(Teacher.Assignments);

            SUBJECT_NO_ASSIGNMENT = new Error(ErrManager,
                (e) =>
                {
                    IEnumerable<Subject> subjectMismatches = Teacher.Subjects.Except(Teacher.Assignments.Select(a => a.Subject));
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

            ASSIGNMENT_NO_SUBJECT = new Error(ErrManager,
                (e) =>
                {
                    IEnumerable<Assignment> assignmentMismatches = Teacher.Assignments.Where(a => !Teacher.Subjects.Contains(a.Subject));
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

            NOT_ENOUGH_FORM_SLOTS = new Error(ErrManager,
                (e) =>
                {
                    IEnumerable<Lesson> lessons = Teacher.Assignments.Select(a => a.Form.Lessons.Where(l => l.Subject == a.Subject).Single());
                    IEnumerable<Tuple<Form, Subject>> errors = lessons.Where(l => l.LessonsPerCycle < l.Form.Assignments.Where(a => a.Subject == l.Subject).Sum(a => a.LessonCount)).Select(l => new Tuple<Form, Subject>(l.Form, l.Subject));
                    e.Data = errors;
                    return errors.Any();
                },
                (e) =>
                {
                    IEnumerable<Tuple<Form, Subject>> errors = (IEnumerable<Tuple<Form, Subject>>)e.Data;
                    return $"The following combinations of Form/Subjects have more lessons than have been allocated to it: {FormatEnumerable(errors.Select(t => $"{t.Item1}/{t.Item2}"))}.";
                }, ErrorType.Error);
            NOT_ENOUGH_FORM_SLOTS.BindCollection(Teacher.Assignments);

            foreach (Subject subject in Teacher.Subjects)
            {
                AddSubject(subject);
            }
            foreach (Assignment assignment in Teacher.Assignments)
            {
                AddAssignment(assignment);
            }

            string[] days = new string[5] { "Mon", "Tue", "Wed", "Thu", "Fri" };
            for (int week = 0; week < Structure.WeeksPerCycle; week++)
            {
                Grid gridWeek = new Grid()
                {
                    Width = 200
                };
                gridWeek.ColumnDefinitions.Add(new ColumnDefinition());
                gridWeek.RowDefinitions.Add(new RowDefinition());
                gridWeek.Children.Add(SetInternalBorder(new TextBlock()
                {
                    Text = DataHelpers.WeekToString(week),
                    Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFF"),
                    Padding = new Thickness(2),
                    TextAlignment = TextAlignment.Center
                })
                );


                for (int day = 0; day < 5; day++)
                {
                    ColumnDefinition columnDay = new ColumnDefinition()
                    {
                        Width = new GridLength(1, GridUnitType.Star),
                    };
                    gridWeek.ColumnDefinitions.Add(columnDay);

                    TextBlock dayHeading = new TextBlock()
                    {
                        Text = days[day],
                        Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFF"),
                        Padding = new Thickness(2),
                        TextAlignment = TextAlignment.Center
                    };
                    Border dayBorder = SetInternalBorder(dayHeading);
                    Grid.SetColumn(dayBorder, day + 1);
                    gridWeek.Children.Add(dayBorder);
                }

                for (int periodCount=0; periodCount<Structure.Structure.Count; periodCount++)
                {
                    TimetableStructurePeriod period = Structure.Structure[periodCount];
                    RowDefinition rowPeriod = new RowDefinition()
                    {
                        //Height = new GridLength(1, GridUnitType.Star)
                    };
                    gridWeek.RowDefinitions.Add(rowPeriod);

                    TextBlock periodHeading = new TextBlock()
                    {
                        Text = period.Name,
                        Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFF"),
                        Padding = new Thickness(2)
                    };
                    Border periodBorder = SetInternalBorder(periodHeading);
                    Grid.SetRow(periodBorder, periodCount + 1);
                    gridWeek.Children.Add(periodBorder);

                    for (int day = 0; day < 5; day++)
                    {
                        bool schedulable = period.IsSchedulable;
                        TimetableSlot slot = new TimetableSlot(week, day, periodCount);
                        bool isUnavailable = Teacher.UnavailablePeriods.Contains(slot);
                        Rectangle rect = new Rectangle()
                        {
                            Fill = (SolidColorBrush)new BrushConverter().ConvertFromString(schedulable ?
                            (isUnavailable ? "#FF0000" : "#00FF00") :
                            "#A8A8A8"),
                            Tag = schedulable ? Tuple.Create(slot, !isUnavailable) : null
                        };
                        if (schedulable)
                        {
                            rect.MouseLeftButtonDown += ToggleSlot;
                        }
                        Border rectBorder = SetInternalBorder(rect);
                        Grid.SetColumn(rectBorder, day + 1);
                        Grid.SetRow(rectBorder, periodCount + 1);
                        gridWeek.Children.Add(rectBorder);
                    }
                }
                gridWeek.MouseRightButtonDown += ToggleAll;
                spPeriods.Children.Add(new Border()
                {
                    Child = gridWeek,
                    Style = (Style)Application.Current.Resources["GridLineExternal"],
                    Margin = new Thickness(0, 5, 10, 5)
                });
            }
        }

        private void SubjectButtonClick(object sender, RoutedEventArgs e)
        {
            
            Subject subject = (Subject)cmbxSubjects.SelectedItem;
            if (subject == null)
            {
                if (string.IsNullOrWhiteSpace(cmbxSubjects.Text))
                {
                    return;
                }
                subject = new Subject() { Name = cmbxSubjects.Text.Trim() };
                subject.Commit();
            }
            else
            {
                if (Teacher.Subjects.Contains(subject))
                {
                    return;
                }
            }
            AddSubject(subject);
            cmbxSubjects.SelectedItem = subject;
            Teacher.Subjects.Add(subject);
        }

        private void AssignmentButtonClick(object sender, RoutedEventArgs e)
        {
            Form form = (Form)cmbxAssignmentForm.SelectedItem;
            int? lessons = iupdown.Value;
            Subject subject = (Subject)cmbxAssignmentSubject.SelectedItem;
            if (form == null || lessons == null)
            {
                return;
            }
            Assignment old = Teacher.Assignments.Where(a => a.Subject == subject && a.Form == form).SingleOrDefault();
            if (old != null)
            {
                if (old.LessonCount == lessons) { return; }
                RemoveAssignment(old);
            }
            Assignment assignment = new Assignment(form, (int)lessons, subject);
            AddAssignment(assignment);
            Teacher.Assignments.Add(assignment);
        }

        private void AddSubject(Subject subject)
        {            
            spSubjects.Children.Add(VerticalMenuItem(subject, RemoveSubjectClick));
        }

        private void RemoveSubjectClick(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            StackPanel sp = (StackPanel)element.Parent;
            Subject subject = (Subject)element.Tag;
            Teacher.Subjects.Remove(subject);
            spSubjects.Children.Remove(sp);
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
        public MainPage MainPage { get; set; }
        private readonly TimetableStructure Structure = (TimetableStructure)Application.Current.Properties[TimetableStructure.ListName];
        private readonly Error HAS_NO_PERIODS;
        private readonly Error NOT_ENOUGH_PERIODS;
        private readonly Error SUBJECT_NO_ASSIGNMENT;
        private readonly Error ASSIGNMENT_NO_SUBJECT;
        private readonly Error NOT_ENOUGH_FORM_SLOTS;
        private readonly Error HAS_EMPTY_NAME;
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

        private void CmbxSubjects_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                SubjectButtonClick(null, null);
            }
            if (e.Key == Key.Escape)
            {
                cmbxSubjects.SelectedItem = null;
                cmbxSubjects.Text = "";
                Keyboard.ClearFocus();
            }
        }

        private void CmbxAssignmentsSubjectsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmbx = (ComboBox)sender;
            object subject = (object)cmbx.SelectedItem;
            IEnumerable<Form> all_forms = (IEnumerable<Form>)Application.Current.Properties[Form.ListName];
            if (subject == null)
            {
                cmbxAssignmentForm.ItemsSource = all_forms;
                return;
            }
            IEnumerable<Form> setes = from form in all_forms where form.Lessons.Select(l => l.Subject).Contains(subject) select form;
            cmbxAssignmentForm.ItemsSource = setes;
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
            HAS_EMPTY_NAME.UpdateError();
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
            foreach (Assignment assignment in Teacher.Assignments)
            {
                assignment.Commit(Teacher);
            }
            Teacher.Name = txName.Text;

            if (CommandType == CommandType.edit) {
                OriginalTeacher.Recommit(Teacher);
            } else
            {
                Teacher.Commit();
            }
            
            MainPage.CloseTab(this);
        }
    }
}
