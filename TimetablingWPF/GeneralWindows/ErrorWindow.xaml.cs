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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Humanizer;
using static TimetablingWPF.VisualHelpers;
using ObservableComputations;
using System.Collections.Specialized;

namespace TimetablingWPF
{
    /// <summary>
    /// Interaction logic for ErrorList.xaml
    /// </summary>
    public partial class ErrorWindow : Window
    {
        public ErrorWindow()
        {
            InitializeComponent();
            headerStyle = (Style)Resources["HeaderStyle"];
            tbStyle = (Style)Resources["tbStyle"];
            foreach (Type type in DataHelpers.UserTypes)
            {
                TextBlock header = new TextBlock()
                {
                    Style = headerStyle,
                    Text = type.Name.Pluralize()
                };
                header.SetResourceReference(TextBlock.BackgroundProperty, "PrimaryBrush");
                Headers.Add(header);
                spMain.Children.Add(header);
                Filtering<BaseDataClass> list = ((INotifyCollectionChanged)App.Data.FromType(type)).Casting<BaseDataClass>().Filtering(
                    o => o.Visible && o.ErrorValidations.Any(e => e.IsErroredState)
                );
                Console.WriteLine(list.Count);
                for (int i = 0; i < list.Count; i++)
                {
                    BaseDataClass item = list[i];
                    if (!item.Visible)
                    {
                        continue;
                    }
                    TextBlock tbItem = new TextBlock()
                    {
                        Text = item.ToString(),
                        Style = tbStyle
                    };
                    Console.WriteLine(item);
                    Border border = new Border()
                    {
                        Child = tbItem,
                        BorderBrush = GenericResources.BLACK
                    };
                    if (i != 0)
                    {
                        border.BorderThickness = new Thickness(0, 1, 0, 0);
                    }
                    spMain.Children.Add(border);
                }
            }
        }
        private TextBlock GenerateFloatingHeader(string text, int index)
        {
            TextBlock tb = new TextBlock()
            {
                Text = text,
                Style = headerStyle,
                Background = ((SolidColorBrush)FindResource("PrimaryBrush")).Clone(),
                Cursor = Cursors.Hand,
                Tag = index
            };
            tb.MouseEnter += AnimateMouseEnter;
            tb.MouseLeave += AnimateMouseLeave;
            tb.MouseDown += AnimateClick;
            return tb;
        }

        private void AnimateClick(object sender, MouseButtonEventArgs e)
        {
            TextBlock header = (TextBlock)sender;
            int index = (int)header.Tag;
            TextBlock target = Headers[index];
            double scroll = svMain.VerticalOffset + target.TransformToAncestor(svMain).Transform(origin).Y - (
                header.RenderSize.Height * index + border_thickness * Math.Max(index - 1, 0));
            svMain.BeginAnimation(AnimatableScrollViewer.AnimatableVerticalOffsetProperty, new DoubleAnimation(svMain.VerticalOffset, scroll, scroll_time.ToMillisDuration()) { EasingFunction = new CubicEase() });

        }

        private const int fade_time = 150;
        private const double fade_factor = 0.75;
        private const int scroll_time = 450;
        private const int border_thickness = 2;
        private readonly Point origin = new Point(0, 0);
        private void AnimateMouseEnter(object sender, MouseEventArgs e)
        {
            TextBlock tb = (TextBlock)sender;
            Color to = TintColour((Color)FindResource("PrimaryColor"), fade_factor);
            ColorAnimation ca = new ColorAnimation(to, fade_time.ToMillisDuration());
            tb.Background.BeginAnimation(SolidColorBrush.ColorProperty, ca);
        }
        private void AnimateMouseLeave(object sender, MouseEventArgs e)
        {
            TextBlock tb = (TextBlock)sender;
            Color to = (Color)FindResource("PrimaryColor");
            ColorAnimation ca = new ColorAnimation(to, fade_time.ToMillisDuration());
            tb.Background.BeginAnimation(SolidColorBrush.ColorProperty, ca);
        }

        private readonly IList<TextBlock> Headers = new List<TextBlock>();
        private readonly Style headerStyle;
        private readonly Style tbStyle;
        private int InView(FrameworkElement visual)
        {
            double height = visual.TransformToAncestor(svMain).Transform(origin).Y;
            double mod = visual.ActualHeight + border_thickness;
            if (height < spTop.ActualHeight - mod)
            {
                return 2;
            }
            if (height < spTop.ActualHeight)
            {
                return 1;
            }
            if (height > svMain.ActualHeight - spBottom.ActualHeight + mod)
            {
                return -2;
            }
            if (height > svMain.ActualHeight - spBottom.ActualHeight)
            {
                return -1;
            }
            return 0;
        }
        private void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            int dir = Math.Sign(e.VerticalChange);
            if (dir != -1)
            {
                for (int i = 0; i < Headers.Count; i++)
                {
                    TextBlock header = Headers[i];
                    int pos = InView(header);
                    if (pos > 0 && spTop.Children.Count <= i)
                    {
                        Border border = new Border()
                        {
                            Child = GenerateFloatingHeader(header.Text, i),
                            BorderThickness = new Thickness(0, border_thickness, 0, 0)
                        };
                        border.SetResourceReference(Border.BorderBrushProperty, "SecondaryBrush");
                        spTop.Children.Add(border);
                    }
                    if (pos > -2 && spBottom.Children.Count >= Headers.Count - i)
                    {
                        spBottom.Children.RemoveAt(0);
                    }
                }
            }
            if (dir != 1) {
                for (int i = Headers.Count - 1; i >= 0; i--)
                {
                    TextBlock header = Headers[i];
                    int pos = InView(header);
                    if (pos < 0 && spBottom.Children.Count < Headers.Count - i)
                    {
                        Border border = new Border()
                        {
                            Child = GenerateFloatingHeader(header.Text, i)
                        };
                        border.SetResourceReference(Border.BorderBrushProperty, "SecondaryBrush");
                        if (spBottom.Children.Count > 0)
                        {
                            border.BorderThickness = new Thickness(0, 0, 0, border_thickness);
                        }
                        spBottom.Children.Insert(0, border);
                    }
                    if (((i == 0 && svMain.VerticalOffset == 0) || (i != 0 && pos < 2)) && spTop.Children.Count > i)
                    {
                        spTop.Children.RemoveAt(spTop.Children.Count - 1);
                    }
                }
            }
        }
    }
}
