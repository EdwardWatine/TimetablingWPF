using System;
using System.Collections.Generic;
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
using static TimetablingWPF.VisualHelpers;
using TimetablingWPF.StructureClasses;

namespace TimetablingWPF
{
    /// <summary>
    /// Interaction logic for TimetableStructureDialog.xaml
    /// </summary>
    public partial class TimetableStructureDialog : Window
    {
        public TimetableStructureDialog(Window owner, bool showWarning)
        {
            InitializeComponent();
            Owner = owner;
            ShowWarning = showWarning;
            Populate();
        }
        private readonly bool ShowWarning; // This is true if the current timetable is being changed (vs making a new one)
        public void Populate()
        {
            for (int week = 0; week < TimetableStructure.Weeks.Count; week++)
            {
                spWeeks.Children.Add(GenerateWeek(TimetableStructure.Weeks[week])); // generate each week
            }
            Button newWeek = new Button() // button to add a new week
            {
                Content = new Image { Source = (ImageSource)Application.Current.Resources["PlusIcon"] },
                Height = 40,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(10)
            };
            newWeek.Click += NewWeekClick;
            spWeeks.Children.Add(newWeek);
        }

        private Grid GenerateWeek(TimetableStructureWeek structureWeek)
        {
            Grid gridWeek = new Grid()
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 5, 10, 5)
            };
            gridWeek.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
            gridWeek.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
            gridWeek.RowDefinitions.Add(new RowDefinition()); // set up container grid
            Border weekHeader = SetInternalBorder(new EditableText() // create the week name
            {
                Text = structureWeek.Name,
                Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFF"),
                Padding = new Thickness(2),
                TextAlignment = TextAlignment.Center
            });
            Grid.SetColumn(weekHeader, 1);
            gridWeek.Children.Add(weekHeader);


            for (int day = 0; day < structureWeek.DayNames.Count; day++)
            {
                ColumnDefinition columnDay = new ColumnDefinition()
                {
                    Width = new GridLength(1, GridUnitType.Auto),
                };
                gridWeek.ColumnDefinitions.Add(columnDay);

                EditableText dayHeading = new EditableText() // add the day name
                {
                    Text = structureWeek.DayNames[day],
                    Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFF"),
                    Padding = new Thickness(2),
                    Tag = gridWeek
                };
                Border dayBorder = SetInternalBorder(dayHeading);
                Grid.SetColumn(dayBorder, day + 2);
                gridWeek.Children.Add(dayBorder);
            }

            for (int period = 0; period < structureWeek.PeriodNames.Count; period++)
            {
                RowDefinition rowPeriod = new RowDefinition()
                {
                    //Height = new GridLength(1, GridUnitType.Star)
                };
                gridWeek.RowDefinitions.Add(rowPeriod);
                EditableText periodHeading = new EditableText() // add the period names
                {
                    Text = structureWeek.PeriodNames[period],
                    Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFF"),
                    Padding = new Thickness(2),
                    Tag = gridWeek
                };
                Border periodBorder = SetInternalBorder(periodHeading);
                Grid.SetRow(periodBorder, period + 1);
                Grid.SetColumn(periodBorder, 1);
                gridWeek.Children.Add(periodBorder);
                gridWeek.Children.Add(GenerateCross(period + 1));
                for (int day = 0; day < structureWeek.DayNames.Count; day++)
                {
                    bool schedulable = structureWeek.PeriodIsSchedulable(day, period);
                    Rectangle rect = new Rectangle() // add the individual period
                    {
                        Fill = (SolidColorBrush)new BrushConverter().ConvertFromString(schedulable ? "#00FF00" : "#A8A8A8"),
                        Tag = schedulable
                    };
                    rect.MouseLeftButtonDown += RectLeftClick;
                    rect.MouseEnter += MouseEntered; // add the appropriate mouse bindings
                    Border rectBorder = SetInternalBorder(rect);
                    Grid.SetColumn(rectBorder, day + 2);
                    Grid.SetRow(rectBorder, period + 1);
                    gridWeek.Children.Add(rectBorder);
                }
            }
            Button newPeriod = new Button() // add the button that adds a new period
            {
                Content = new Image() { Source = (ImageSource)Application.Current.Resources["PlusIcon"] },
                Tag = gridWeek,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Height = 20,
                Margin = new Thickness(5)
            };
            newPeriod.Click += NewPeriodClick;
            gridWeek.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
            Grid.SetRow(newPeriod, gridWeek.RowDefinitions.Count - 1);
            gridWeek.Children.Add(newPeriod);
            Image removeWeek = new Image() // add the image that removes the week
            {
                Source = (ImageSource)Application.Current.Resources["WhiteBinIcon"],
                HorizontalAlignment = HorizontalAlignment.Center,
                Height = 30,
                Cursor = Cursors.Hand
            };
            Grid.SetColumnSpan(removeWeek, structureWeek.DayNames.Count + 1);
            Grid.SetColumn(removeWeek, 1);
            Grid.SetRow(removeWeek, gridWeek.RowDefinitions.Count - 1);
            removeWeek.MouseDown += RemoveWeekClick;
            gridWeek.Children.Add(removeWeek);
            return gridWeek;
        }

        private void RemoveWeekClick(object sender, MouseButtonEventArgs e)
        {
            Grid grid = (Grid)((FrameworkElement)sender).Parent;
            if (spWeeks.Children.Count == 2)
            {
                ShowErrorBox("There must be at least one week in the timetable");
                return;
            }
            spWeeks.Children.Remove(grid);
        }

        private void NewWeekClick(object sender, RoutedEventArgs e)
        {
            Grid newWeek = GenerateWeek(new TimetableStructureWeek("<New Week>", DataHelpers.ShortenedDaysOfTheWeek, TimetableStructure.Weeks[0].PeriodNames, new List<int>()));
            spWeeks.Children.Insert(spWeeks.Children.Count - 1, newWeek);
        }

        private void RemovePeriodClick(object sender, MouseButtonEventArgs e)
        {
            Image cross = (Image)sender;
            Grid grid = (Grid)cross.Parent;
            int count = Grid.GetRow(cross);
            if (grid.RowDefinitions.Count == 3)
            {
                ShowErrorBox("A week must have periods in it.");
                return;
            }
            System.Collections.IList children = grid.Children;
            for (int i = 0; i < children.Count; i++)
            {
                UIElement element = (UIElement)children[i];
                int row = Grid.GetRow(element);
                if (row == count) // remove the element if it is on the correct row
                {
                    children.Remove(element);
                    i--;
                }
                if (row > count)
                {
                    Grid.SetRow(element, row - 1); // shifts other elements down one
                }
            }
            grid.RowDefinitions.RemoveAt(count);
        }

        private void RectLeftClick(object sender, MouseButtonEventArgs e)
        {
            Rectangle rect = (Rectangle)sender;
            bool current_availability = !(bool)rect.Tag;
            rect.Fill = (SolidColorBrush)new BrushConverter().ConvertFromString(current_availability ? "#00FF00" : "#A8A8A8");
            rect.Tag = current_availability;
        }
        private void DayClick(object sender, MouseButtonEventArgs e)
        {
            IList<Rectangle> rects = new List<Rectangle>();
            FrameworkElement dayHeader = (FrameworkElement)sender;
            Grid grid = (Grid)((FrameworkElement)sender).Tag;
            int col = Grid.GetColumn((UIElement)dayHeader.Parent);
            bool all_uv = true;
            foreach (Border border in grid.Children.Cast<Border>().Where(b => Grid.GetColumn(b) == col))
            {
                if (border.Child is Rectangle rect)
                {
                    if ((bool)rect.Tag)
                    {
                        all_uv = false;
                        RectLeftClick(rect, null);
                        continue;
                    }
                    rects.Add(rect);
                }
            }
            if (!all_uv)
            {
                return;
            }
            foreach (Rectangle rect in rects)
            {
                RectLeftClick(rect, null);
            }
        }
        private void MouseEntered(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                RectLeftClick(sender, null);
            }
        }
        private void PeriodClick(object sender, MouseButtonEventArgs e)
        {
            IList<Rectangle> rects = new List<Rectangle>();
            FrameworkElement periodHeader = (FrameworkElement)sender;
            Grid grid = (Grid)((FrameworkElement)sender).Tag;
            int row = Grid.GetRow((UIElement)periodHeader.Parent);
            bool all_uv = true;
            foreach (Border border in grid.Children.Cast<Border>().Where(b => Grid.GetRow(b) == row))
            {
                if (border.Child is Rectangle rect)
                {
                    if ((bool)rect.Tag)
                    {
                        all_uv = false;
                        RectLeftClick(rect, null);
                        continue;
                    }
                    rects.Add(rect);
                }
            }
            if (!all_uv)
            {
                return;
            }
            foreach (Rectangle rect in rects)
            {
                RectLeftClick(rect, null);
            }
        }
        private void NewPeriodClick(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Grid grid = (Grid)button.Tag;
            int length = grid.RowDefinitions.Count;
            grid.RowDefinitions.Add(new RowDefinition());
            Grid.SetRow(button, length);
            EditableText periodHeading = new EditableText()
            {
                Text = "<New Period>",
                Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFF"),
                Padding = new Thickness(2),
                Tag = grid
            };
            periodHeading.MouseLeftButtonDown += PeriodClick;
            Border periodBorder = SetInternalBorder(periodHeading);
            Grid.SetRow(periodBorder, length-1);
            Grid.SetColumn(periodBorder, 1);
            grid.Children.Add(periodBorder);
            grid.Children.Add(GenerateCross(length-1));
            int days = grid.ColumnDefinitions.Count - 1;
            for (int day = 0; day < days; day++)
            {
                Rectangle rect = new Rectangle()
                {
                    Fill = (SolidColorBrush)new BrushConverter().ConvertFromString("#00FF00"),
                    Tag = true
                };
                rect.MouseLeftButtonDown += RectLeftClick;
                Border rectBorder = SetInternalBorder(rect);
                Grid.SetColumn(rectBorder, day + 2);
                Grid.SetRow(rectBorder, length - 1);
                grid.Children.Add(rectBorder);
            }
        }
        private Image GenerateCross(int row)
        {
            Image removePeriod = new Image() { Source = (ImageSource)Application.Current.Resources["CrossIcon"], Height = 20 };
            removePeriod.Cursor = Cursors.Hand;
            removePeriod.MouseDown += RemovePeriodClick;
            Grid.SetRow(removePeriod, row);
            return removePeriod;
        }
        private void SaveData()
        {
            if (ShowWarning && ShowWarningBox("All data related to the timetable will be deleted. Are you sure you want to continue?") == MessageBoxResult.Cancel)
            {
                return;
            }
            string weekName = null;
            IList<string> periods = new List<string>();
            IList<string> days = new List<string>();
            IList<int> uv_periods = new List<int>();
            IList<TimetableStructureWeek> weeks = new List<TimetableStructureWeek>();
            foreach (UIElement grid_element in spWeeks.Children)
            {
                periods.Clear();
                days.Clear();
                uv_periods.Clear(); //resets the lists for each iteration
                if (!(grid_element is Grid grid)) { continue; }
                foreach (UIElement parent_element in grid.Children)
                {
                    int row = Grid.GetRow(parent_element), column = Grid.GetColumn(parent_element) - 1;
                    if (!(parent_element is Border border)) { continue; }
                    UIElement element = border.Child;
                    if (row == 0)
                    {
                        if (column == 0)
                        {
                            weekName = ((EditableText)element).Text;
                            continue;
                        }
                        days.InsertDefaultIndex(column - 1, ((EditableText)element).Text);
                        continue;
                    }
                    if (column == 0)
                    {
                        periods.InsertDefaultIndex(row - 1, ((EditableText)element).Text);
                        continue;
                    }
                    if (!(bool)((FrameworkElement)element).Tag)
                    {
                        uv_periods.Add(column - 1);
                        uv_periods.Add(row - 1);
                    }
                }
                int num_periods = periods.Count;
                weeks.Add(new TimetableStructureWeek(weekName, new List<string>(days), new List<string>(periods), // adds the correct weeks to the list
                    Enumerable.Range(0, uv_periods.Count / 2).Select(i => TimetableStructureWeek.IndexesToPeriodNum(uv_periods[2*i], uv_periods[2*i + 1], num_periods)).ToList())); // generates the days and the periods
            }
            TimetableStructure.SetData(weeks); // set the current structure data
        }
        public void Confirm(object sender, RoutedEventArgs e)
        {
            SaveData();
            DialogResult = true;

        }
    }
}
