using Humanizer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace TimetablingWPF
{
    /// <summary>
    /// Interaction logic for TeacherTab.xaml
    /// </summary>
    public partial class TeacherTab : Page
    {
        public TeacherTab(Teacher teacher = null)
        {
            InitializeComponent();
            tbTitle.Text = "Create a new Teacher";
            txName.Text = teacher?.Name;
            UnavailablePeriods = teacher?.UnavailablePeriods ?? new ObservableCollection<TimetableSlot>();
            Subjects = teacher?.Subjects ?? new ObservableCollection<Subject>();
            Assignments = teacher?.Assignments ?? new ObservableCollection<Assignment>();

            dmSubjects.Dataset = (IList<Subject>)Application.Current.Properties["Subjects"];
            dmAssignments.Dataset = (IList<Subject>)Application.Current.Properties["Subjects"];
            dmAssignments.function = x => Tuple.Create(x, 0);


            TimetableStructure structure = (TimetableStructure)Application.Current.Properties["Structure"];
            string[] days = new string[5] { "Mon", "Tue", "Wed", "Thu", "Fri" };
            for (int week = 0; week < structure.WeeksPerCycle; week++)
            {
                Grid gridWeek = new Grid()
                {
                    Width = 200
                };
                gridWeek.ColumnDefinitions.Add(new ColumnDefinition());
                gridWeek.RowDefinitions.Add(new RowDefinition());

                gridWeek.Children.Add(Utility.setInternalBorder(new TextBlock()
                {
                    Text = Convert.ToString((char)('A' + week)),
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

                int periodCount = 0;
                foreach (TimetableStructurePeriod period in structure.Structure)
                {
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
                        Rectangle rect = new Rectangle()
                        {
                            Fill = (SolidColorBrush)new BrushConverter().ConvertFromString(schedulable ? "#00FF00" : "#A8A8A8"),
                            Tag = schedulable ? Tuple.Create(new TimetableSlot(week, day, periodCount), true) : null
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

                    periodCount++;
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

        private void TriggerSubjectCbox(object sender, MouseButtonEventArgs e)
        {
            CheckBox cbox = (CheckBox)((StackPanel)sender).Tag;
            cbox.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
        }

        private void ToggleGroup(object sender, RoutedEventArgs e)
        {
            CheckBox cbox = (CheckBox)sender;
            Tuple<Subject, bool> tag = (Tuple<Subject, bool>)cbox.Tag;
            cbox.Tag = Tuple.Create(tag.Item1, !tag.Item2);
            if (tag.Item2)
            {
                Subjects.Remove(tag.Item1);
                return;
            }
            Subjects.Add(tag.Item1);
        }

        private IList<TimetableSlot> UnavailablePeriods;
        private IList<Subject> Subjects;
        private IList<Assignment> Assignments;
        public MainPage MainPage = (MainPage)Application.Current.MainWindow.Content;

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
                UnavailablePeriods.Add(tag.Item1);
                return;
            }
            UnavailablePeriods.Remove(tag.Item1);
        }
    }
}
