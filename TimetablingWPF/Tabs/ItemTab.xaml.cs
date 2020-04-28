using System;
using System.Collections;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;
using static TimetablingWPF.VisualHelpers;
using static TimetablingWPF.DataHelpers;
using System.Globalization;
using System.Collections.Specialized;

namespace TimetablingWPF
{
    /// <summary>
    /// Interaction logic for ItemTab.xaml
    /// </summary>
    public partial class ItemTab : TabBase
    {
        public ItemTab(BaseDataClass originalItem, CommandType commandType)
        {
            InitializeComponent();
            ErrManager = new ErrorManager(spErrors);
            CommandType = commandType;
            Item = commandType == CommandType.@new ? originalItem : (BaseDataClass)originalItem.Clone();
            OriginalItem = originalItem;
            Item.Freeze();
            txName.Text = originalItem.Name;
            txName.SelectionStart = txName.Text.Length;
            int iters = 0;
            Grid gdRight = new Grid();
            Type item_type = originalItem.GetType();
            foreach (CustomPropertyInfo prop in BaseDataClass.ExposedProperties[item_type])
            {
                TextBlock propName = new TextBlock()
                {
                    Text = prop.Alias + ":",
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Style = (Style)Application.Current.Resources["DialogText"]
                };
                bool is_timetable = prop.Type == typeof(ObservableCollection<TimetableSlot>);
                if (prop.Type.IsInterface<IList>() && !is_timetable)
                {
                    if (iters == 0)
                    {
                        gdMain.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                        gdRight.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
                        gdRight.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                        gdMain.Insert(gdRight, 1, 1);
                    }
                    iters++;
                    Grid gdilContainer = new Grid();
                    if (iters == 3)
                    {
                        gdLeft.RowDefinitions.SmartInsert(-2, new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                        gdLeft.Insert(propName, -2, 0);
                        gdLeft.Insert(gdilContainer, -2, 1);
                    }
                    else
                    {
                        gdRight.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                        gdRight.Insert(propName, -1, 0);
                        gdRight.Insert(gdilContainer, -1, 1);
                    }
                    gdilContainer.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                    gdilContainer.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
                    ItemList itemlist = new ItemList(prop)
                    {
                        ItemsSource = (IList)prop.PropertyInfo.GetValue(Item)
                    };
                    if (prop.Type.IsInterface<INotifyCollectionChanged>())
                    {
                        itemlist.ListenToCollection((INotifyCollectionChanged)prop.PropertyInfo.GetValue(Item));
                    }
                    gdilContainer.Children.Add(
                        new ScrollViewer()
                        {
                            HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
                            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                            Content = itemlist
                        });
                    StackPanel sphorizontalMenu = new StackPanel()
                    {
                        Orientation = Orientation.Horizontal
                    };
                    Grid.SetRow(sphorizontalMenu, 1);
                    Style hzmenu = (Style)Application.Current.Resources["HorizontalMenu"];
                    MultiComboBox comboBox = new MultiComboBox()
                    {
                        Style = hzmenu,
                    };
                    Type generic_argument = prop.Type.GetGenericArguments()[0];
                    RoutedEventHandler handler = null;
                    if (generic_argument == typeof(Assignment))
                    {
                        Type box_type = item_type == typeof(Teacher) ? typeof(Lesson) : typeof(Teacher);
                        comboBox.ItemsSource = ((INotifyCollectionChanged)GetDataContainer().FromType(box_type)).GenerateOneWayCopy();
                        comboBox.ItemString = box_type.Name.ToLower(CultureInfo.CurrentCulture);
                        sphorizontalMenu.Children.Add(comboBox);
                        IntegerUpDown iupdown = new IntegerUpDown()
                        {
                            Minimum = 1,
                            Value = 1,
                            Style = hzmenu,
                            Width = 50
                        };
                        sphorizontalMenu.Children.Add(iupdown);
                        int? lessonCount = iupdown.Value;
                        if (lessonCount == null)
                        {
                            return;
                        }
                        if (Item is Teacher teacher)
                        {
                            handler = new RoutedEventHandler(
                                (sender, e) =>
                                {
                                    foreach (Lesson lesson in comboBox.SelectedItems)
                                    {
                                        Assignment old = teacher.Assignments.Where(a => a.Lesson == lesson).SingleOrDefault();
                                        if (old != null)
                                        {
                                            if (old.LessonCount == lessonCount)
                                            {
                                                return;
                                            }
                                            teacher.Assignments.Remove(old);
                                        }
                                        Assignment assignment = new Assignment(teacher, lesson, (int)lessonCount);
                                        teacher.Assignments.Add(assignment);
                                    }
                                });
                        }
                        else if (Item is Lesson lesson)
                        {
                            handler = new RoutedEventHandler(
                                (sender, e) =>
                                {
                                    foreach (Teacher teacher_l in comboBox.SelectedItems)
                                    {
                                        Assignment old = lesson.Assignments.Where(a => a.Teacher == teacher_l).SingleOrDefault();
                                        if (old != null)
                                        {
                                            if (old.LessonCount == lessonCount) { return; }
                                            lesson.Assignments.Remove(old);
                                        }
                                        Assignment assignment = new Assignment(teacher_l, lesson, (int)lessonCount);
                                        lesson.Assignments.Add(assignment);
                                    }
                                });
                        }
                        Button button = new Button()
                        {
                            Content = new Image()
                            {
                                Source = (ImageSource)Application.Current.Resources["PlusIcon"]
                            }
                        };

                        button.Click += handler;
                        button.SetBinding(HeightProperty, new Binding("ActualHeight")
                        {
                            Source = comboBox
                        });
                        sphorizontalMenu.Children.Add(button);
                    }
                    else
                    {
                        comboBox.ItemsSource = ((INotifyCollectionChanged)GetDataContainer().FromType(generic_argument)).GenerateOneWayCopy();
                        comboBox.ItemString = generic_argument.Name.ToLower(CultureInfo.CurrentCulture);
                        sphorizontalMenu.Children.Add(comboBox);

                        object value = prop.PropertyInfo.GetValue(Item);
                        comboBox.SetSelected((IEnumerable)value);
                        ((INotifyCollectionChanged)comboBox.SelectedItems).CollectionChanged += GenericHelpers.GenerateLinkHandler((IList)value);
                        itemlist.DeleteAction = o => comboBox.SelectedItems.Remove(o);
                    }
                    gdilContainer.Children.Add(sphorizontalMenu);
                    continue;
                }
                gdLeft.RowDefinitions.SmartInsert(-2, new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
                gdLeft.Insert(propName, -2, 0);
                if (is_timetable)
                {
                    ScrollViewer svPeriods = new ScrollViewer()
                    {
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
                        Margin = new Thickness(0, 5, 0, 5),
                        Content = GenerateTimetable((ObservableCollection<TimetableSlot>)prop.PropertyInfo.GetValue(Item), ToggleSlot, ToggleAll)
                    };
                    gdLeft.Insert(svPeriods, -2, 1);
                    continue;
                }
                if (prop.Type == typeof(int))
                {
                    IntegerUpDown iupdown = new IntegerUpDown()
                    {
                        Minimum = 0,
                        Width = 50,
                        HorizontalAlignment = HorizontalAlignment.Left
                    };
                    iupdown.SetBinding(IntegerUpDown.ValueProperty, new Binding(prop.PropertyInfo.Name)
                    {
                        Source = Item,
                        Mode = BindingMode.TwoWay
                    });
                    gdLeft.Insert(iupdown, -2, 1);
                    iupdown.ValueChanged += delegate (object o, RoutedPropertyChangedEventArgs<object> e) { prop.PropertyInfo.SetValue(Item, e.NewValue); };
                    continue;
                }
                if (prop.Type == typeof(bool))
                {
                    CheckBox cbox = new CheckBox()
                    {
                        HorizontalAlignment = HorizontalAlignment.Left
                    };
                    cbox.SetBinding(System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty, new Binding(prop.PropertyInfo.Name)
                    {
                        Source = Item,
                        Mode = BindingMode.TwoWay
                    });
                    gdLeft.Insert(cbox, -2, 1);
                    cbox.Checked += delegate (object sender, RoutedEventArgs e) { prop.PropertyInfo.SetValue(Item, true); };
                    cbox.Unchecked += delegate (object sender, RoutedEventArgs e) { prop.PropertyInfo.SetValue(Item, true); };
                    continue;
                }
                if (prop.Type.IsSubclassOf(typeof(BaseDataClass)) || prop.Type == typeof(Year))
                {
                    PlaceholderComboBox pcbox = new PlaceholderComboBox()
                    {
                        Placeholder = $"-- {prop.Type.Name} --",
                        IsEditable = true,
                        ItemsSource = GetDataContainer().FromType(prop.Type),
                        Width = 150,
                        HorizontalAlignment = HorizontalAlignment.Left
                    };
                    pcbox.comboBox.SetBinding(System.Windows.Controls.Primitives.Selector.SelectedItemProperty,
                        new Binding(prop.PropertyInfo.Name) { Source = Item, Mode = BindingMode.TwoWay });
                    gdLeft.Insert(pcbox, -2, 1);
                    continue;
                }
                throw new InvalidOperationException($"Property {prop.Alias} has a type {prop.Type} which is not supported by the UI");
            }

        }

        private readonly ErrorManager ErrManager;
        private readonly BaseDataClass OriginalItem;
        private readonly BaseDataClass Item;
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
            Teacher teacher = (Teacher)Item;
            bool max = teacher.AvailablePeriods == teacher.MaxPeriodsPerCycle;
            Rectangle rect = (Rectangle)sender;
            Tuple<TimetableSlot, bool> tag = (Tuple<TimetableSlot, bool>)rect.Tag;
            rect.Tag = Tuple.Create(tag.Item1, !tag.Item2);
            rect.Fill = (SolidColorBrush)new BrushConverter().ConvertFromString(tag.Item2 ? "#FF0000" : "#00FF00");
            if (tag.Item2)
            {
                teacher.UnavailablePeriods.Add(tag.Item1);
                teacher.MaxPeriodsPerCycle = Math.Min(teacher.MaxPeriodsPerCycle, teacher.AvailablePeriods);
            }
            else
            {
                teacher.UnavailablePeriods.Remove(tag.Item1);
                if (max) teacher.MaxPeriodsPerCycle++;
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
            Item.Name = txName.Text.Trim();
            if (CommandType == CommandType.edit)
            {
                OriginalItem.UpdateWithClone(Item);
                OriginalItem.Unfreeze();
            }
            else
            {
                Item.Commit();
                Item.Unfreeze();
            }

            
            MainPage.CloseTab(this);
        }
    }
}
