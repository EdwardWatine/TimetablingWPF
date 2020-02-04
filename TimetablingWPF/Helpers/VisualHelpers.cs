﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using static TimetablingWPF.DataHelpers;

namespace TimetablingWPF
{
    class VisualHelpers
    {

        public static StackPanel GenerateTimetable(IEnumerable<TimetableSlot> slots, MouseButtonEventHandler leftClickHandler = null,
            MouseButtonEventHandler rightClickHandler = null)
        {
            StackPanel returnPanel = new StackPanel() { Orientation = Orientation.Horizontal };
            for (int week = 0; week < TimetableStructure.Weeks.Count; week++)
            {
                StructureClasses.TimetableStructureWeek structureWeek = TimetableStructure.Weeks[week];
                Grid gridWeek = new Grid()
                {
                    Width = 200
                };
                gridWeek.ColumnDefinitions.Add(new ColumnDefinition());
                gridWeek.RowDefinitions.Add(new RowDefinition());
                gridWeek.Children.Add(SetInternalBorder(new TextBlock()
                {
                    Text = structureWeek.Name,
                    Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFF"),
                    Padding = new Thickness(2),
                    TextAlignment = TextAlignment.Center
                })
                );


                for (int day = 0; day < structureWeek.DayNames.Count; day++)
                {
                    if (!structureWeek.DayIsSchedulable(day)) { continue; }
                    ColumnDefinition columnDay = new ColumnDefinition()
                    {
                        Width = new GridLength(1, GridUnitType.Star),
                    };
                    gridWeek.ColumnDefinitions.Add(columnDay);

                    TextBlock dayHeading = new TextBlock()
                    {
                        Text = structureWeek.DayNames[day],
                        Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFF"),
                        Padding = new Thickness(2),
                        TextAlignment = TextAlignment.Center
                    };
                    Border dayBorder = SetInternalBorder(dayHeading);
                    Grid.SetColumn(dayBorder, day + 1);
                    gridWeek.Children.Add(dayBorder);
                }

                for (int period = 0; period < structureWeek.PeriodNames.Count; period++)
                {
                    RowDefinition rowPeriod = new RowDefinition()
                    {
                        //Height = new GridLength(1, GridUnitType.Star)
                    };
                    gridWeek.RowDefinitions.Add(rowPeriod);

                    TextBlock periodHeading = new TextBlock()
                    {
                        Text = structureWeek.PeriodNames[period],
                        Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFF"),
                        Padding = new Thickness(2)
                    };
                    Border periodBorder = SetInternalBorder(periodHeading);
                    Grid.SetRow(periodBorder, period + 1);
                    gridWeek.Children.Add(periodBorder);

                    for (int day = 0; day < structureWeek.DayNames.Count; day++)
                    {
                        if (!structureWeek.DayIsSchedulable(day)) { continue; }
                        bool schedulable = structureWeek.PeriodIsSchedulable(day, period);
                        TimetableSlot slot = new TimetableSlot(week, day, period);
                        bool isUnavailable = slots.Contains(slot);
                        Rectangle rect = new Rectangle()
                        {
                            Fill = (SolidColorBrush)new BrushConverter().ConvertFromString(schedulable ?
                            (isUnavailable ? "#FF0000" : "#00FF00") :
                            "#A8A8A8"),
                            Tag = schedulable ? Tuple.Create(slot, !isUnavailable) : null
                        };
                        if (schedulable && leftClickHandler != null)
                        {
                            rect.MouseLeftButtonDown += leftClickHandler;
                        }
                        Border rectBorder = SetInternalBorder(rect);
                        Grid.SetColumn(rectBorder, day + 1);
                        Grid.SetRow(rectBorder, period + 1);
                        gridWeek.Children.Add(rectBorder);
                    }
                }
                if (rightClickHandler != null)
                {
                    gridWeek.MouseRightButtonDown += rightClickHandler;
                }
                returnPanel.Children.Add(new Border()
                {
                    Child = gridWeek,
                    Style = (Style)Application.Current.Resources["GridLineExternalBlack"],
                    Margin = new Thickness(0, 5, 10, 5)
                });
            }
            return returnPanel;
        }
        public static Border SetInternalBorder(FrameworkElement element)
        {
            return new Border()
            {
                Child = element,
                Style = (Style)Application.Current.Resources["GridLineInternalBlack"]
            };
        }
        public static StackPanel VerticalMenuItem(object @object, MouseButtonEventHandler @event = null, string repr = null)
        {
            StackPanel sp = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 5) };
            TextBlock tb = new TextBlock()
            {
                Style = (Style)Application.Current.Resources["DialogText"],
                Text = repr ?? @object.ToString(),
                Margin = new Thickness(0)
            };
            sp.Children.Add(tb);

            if (@event != null)
            {
                Binding binding = new Binding("ActualHeight") { Source = tb };
                Image img = new Image()
                {
                    Source = (ImageSource)Application.Current.Resources["CrossIcon"],
                    Tag = @object,
                    Cursor = Cursors.Hand,
                    Margin = new Thickness(-5, 0, 0, 0)
                };
                img.SetBinding(FrameworkElement.HeightProperty, binding);
                img.MouseDown += @event;
                sp.Children.Add(img);
            }
            return sp;
        }
        public static void ShowErrorBox(string msg, string title = "Error")
        {
            MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        public static MessageBoxResult ShowWarningBox(string msg, string title = "Warning")
        {
            return MessageBox.Show(msg, title, MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel);
        }
        public static MessageBoxResult ShowUnsavedBox()
        {
            return MessageBox.Show("Your data has not been saved. Save the file?", "Unsaved Data", MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation);
        }
    }
}
