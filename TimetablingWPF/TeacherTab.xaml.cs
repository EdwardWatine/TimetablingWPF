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

namespace TimetablingWPF
{
    /// <summary>
    /// Interaction logic for TeacherTab.xaml
    /// </summary>
    public partial class TeacherTab : Page
    {
        public TeacherTab(Teacher teacher, CommandType commandType)
        {
            InitializeComponent();
            ErrManager = new ErrorManager(spErrors);
            CommandType = commandType;
            OriginalTeacher = teacher;
            Teacher = commandType == CommandType.@new ? teacher : (Teacher)teacher.Clone();
            tbTitle.Text = "Create a new Teacher";
            txName.Text = teacher.Name;
            txName.SelectionStart = txName.Text.Length;
            cmbxSubjects.ItemsSource = (IEnumerable<Subject>)Application.Current.Properties["Subjects"];
            cmbxAssignmentSubject.ItemsSource = Teacher.Subjects;
            cmbxAssignmentClass.ItemsSource = (IEnumerable<Class>)Application.Current.Properties["Classes"];
            cmbxAssignmentSubject.comboBox.SelectionChanged += CmbxAssignmentsSubjectsSelectionChanged;

            ErrManager.AddError(HAS_NO_PERIODS, Teacher.UnavailablePeriods.Count == Structure.TotalFreePeriods);
            ErrManager.AddError(NOT_ENOUGH_PERIODS);
            ErrManager.AddError(HAS_EMPTY_NAME);

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
                gridWeek.Children.Add(Utility.setInternalBorder(new TextBlock()
                {
                    Text = Utility.WeekToString(week),
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
                    Border dayBorder = Utility.setInternalBorder(dayHeading);
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
                    Border periodBorder = Utility.setInternalBorder(periodHeading);
                    Grid.SetRow(periodBorder, periodCount + 1);
                    gridWeek.Children.Add(periodBorder);

                    for (int day = 0; day < 5; day++)
                    {
                        bool schedulable = period.IsSchedulable;
                        TimetableSlot slot = new TimetableSlot(week, day, periodCount);
                        Rectangle rect = new Rectangle()
                        {
                            Fill = (SolidColorBrush)new BrushConverter().ConvertFromString(schedulable ?
                            (Teacher.UnavailablePeriods.Contains(slot) ? "#FF0000" : "#00FF00") :
                            "#A8A8A8"),
                            Tag = schedulable ? Tuple.Create(slot, true) : null
                        };
                        if (schedulable)
                        {
                            rect.MouseLeftButtonDown += ToggleSlot;
                        }
                        Border rectBorder = Utility.setInternalBorder(rect);
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
            Class @class = (Class)cmbxAssignmentClass.SelectedItem;
            int? periods = iupdown.Value;
            if (@class == null || periods == null)
            {
                return;
            }
            Assignment assignment = new Assignment(@class, (int)periods);
            AddAssignment(assignment);
            ErrManager.UpdateError(NOT_ENOUGH_PERIODS, (Structure.TotalFreePeriods - Teacher.UnavailablePeriods.Count) < Teacher.Assignments.Sum(x => x.Periods));
            Teacher.Assignments.Add(assignment);
        }

        private void AddSubject(Subject subject)
        {            
            spSubjects.Children.Add(Utility.verticalMenuItem(subject, RemoveSubject));
        }

        private void RemoveSubject(object sender, RoutedEventArgs e)
        {
            StackPanel sp = (StackPanel)((FrameworkElement)sender).Tag;
            Subject subject = (Subject)sp.Tag;
            Teacher.Subjects.Remove(subject);
            spSubjects.Children.Remove(sp);
            ErrManager.UpdateError(NOT_ENOUGH_PERIODS, (Structure.TotalFreePeriods - Teacher.UnavailablePeriods.Count) < Teacher.Assignments.Sum(x => x.Periods));
        }

        private void AddAssignment(Assignment assignment)
        {
            spAssignments.Children.Add(Utility.verticalMenuItem(assignment, RemoveAssignment, assignment.TeacherString));
        }

        private void RemoveAssignment(object sender, RoutedEventArgs e)
        {
            StackPanel sp = (StackPanel)((FrameworkElement)sender).Tag;
            Assignment assignment = (Assignment)sp.Tag;
            Teacher.Assignments.Remove(assignment);
            spAssignments.Children.Remove(sp);
        }
        private readonly Teacher Teacher;
        private readonly Teacher OriginalTeacher;
        public MainPage MainPage = (MainPage)Application.Current.MainWindow.Content;
        private readonly TimetableStructure Structure = (TimetableStructure)Application.Current.Properties["Structure"];
        private readonly Error HAS_NO_PERIODS = new Error("Teacher has no periods", ErrorType.Warning);
        private readonly Error NOT_ENOUGH_PERIODS = new Error("Teacher does not have enough free periods", ErrorType.Error);
        private readonly Error HAS_EMPTY_NAME = new Error("Teacher does not have a name", ErrorType.Error);
        private readonly ErrorManager ErrManager;
        private CommandType CommandType;

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
            ErrManager.UpdateError(HAS_NO_PERIODS, Teacher.UnavailablePeriods.Count == Structure.TotalFreePeriods);
            ErrManager.UpdateError(NOT_ENOUGH_PERIODS, (Structure.TotalFreePeriods - Teacher.UnavailablePeriods.Count) < Teacher.Assignments.Sum(x => x.Periods));
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
            Subject subject = (Subject)cmbx.SelectedItem;
            IEnumerable<Class> all_classes = (IEnumerable<Class>)Application.Current.Properties["Classes"];
            if (subject != null)
            {
                cmbxAssignmentClass.ItemsSource = all_classes;
                return;
            }
            IEnumerable<Class> classes = from @class in all_classes where @class.Subject == subject select @class;
            cmbxAssignmentClass.ItemsSource = classes;
        }

        private void TxNameChanged(object sender, TextChangedEventArgs e)
        {
            ErrManager.UpdateError(HAS_EMPTY_NAME, string.IsNullOrWhiteSpace(txName.Text));
        }

        private void Cancel(object sender, RoutedEventArgs e)
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
                Utility.ShowErrorBox("Please fix all errors!");
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
