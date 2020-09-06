using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static TimetablingWPF.VisualHelpers;

namespace TimetablingWPF
{
    /// <summary>
    /// Interaction logic for BlockingWindow.xaml
    /// </summary>
    public partial class BlockingWindow : Window
    {
        public BlockingWindow()
        {
            InitializeComponent();
            icYear.ItemsSource = App.Data.YearGroups;
            fadeout.Completed += Fadeout_Completed;
        }

        private void Fadeout_Completed(object sender, EventArgs e)
        {
            SetYearData((Year)selected.DataContext);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            animDark = (ColorAnimation)Resources["animDark"];
            animLight = (ColorAnimation)Resources["animLight"];
            animWhite = (ColorAnimation)Resources["animWhite"];
        }
        private Border selected;
        private Border bottom;
        private ColorAnimation animDark;
        private ColorAnimation animLight;
        private ColorAnimation animWhite;
        private readonly ColorAnimation animBlack = new ColorAnimation(GenericResources.BLACK.Color, 200.ToMillisDuration());
        private readonly DoubleAnimation fadein = new DoubleAnimation(1, 200.ToMillisDuration());
        private readonly DoubleAnimation fadeout = new DoubleAnimation(0, 200.ToMillisDuration());
        private void SetYearData(Year year)
        {
            icMain.ItemsSource = year.IndependentSets;
            svMain.BeginAnimation(OpacityProperty, fadein);
        }
        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender != selected)
            {
                ((SolidColorBrush)((Border)sender).Background).BeginAnimation(SolidColorBrush.ColorProperty, animLight);
            }
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender != selected)
            {
                ToNormal((Border)sender);
            }
        }
        private void ToNormal(Border border)
        {
            ((SolidColorBrush)border.Background).BeginAnimation(SolidColorBrush.ColorProperty, animDark);
            ((SolidColorBrush)border.BorderBrush).BeginAnimation(SolidColorBrush.ColorProperty, animBlack);
            SetParentBorder(border);
        }
        private void SetParentBorder(Border border)
        {
            Border parent = (Border)border.Parent;
            Thickness th = new Thickness(0, 0, 0, 1);
            if (IsLastInContainer(border))
            {
                th.Bottom = 0;
            }
            else
            {
                th.Bottom = 1;
            }
            parent.BorderThickness = th;
        }
        private void SetSelected(Border border)
        {
            svMain.BeginAnimation(OpacityProperty, fadeout);
            selected = border;
            ((SolidColorBrush)border.Background).BeginAnimation(SolidColorBrush.ColorProperty, animWhite);
            ((SolidColorBrush)border.BorderBrush).BeginAnimation(SolidColorBrush.ColorProperty, animWhite);
            SetParentBorder(border);
        }
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (selected != sender)
            {
                ToNormal(selected);
                SetSelected((Border)sender);
            }
        }
        private bool IsLastInContainer(Border b)
        {
            return App.Data.YearGroups.IndexOf((Year)b.DataContext) == App.Data.YearGroups.Count - 1;
        }
        private void Border_Loaded(object sender, RoutedEventArgs e)
        {
            Border b = (Border)sender;
            b.BorderBrush = new SolidColorBrush(GenericResources.BLACK.Color); // This is the way it is due to 'freezable' stuff
            if (selected == null)
            {
                SetSelected(b);
            }
            if (IsLastInContainer(b))
            {
                if (bottom != null)
                {
                    SetParentBorder(bottom);
                }
                bottom = b;
                SetParentBorder(b);
            }
        }

        private void ToggleExpander(object sender, MouseButtonEventArgs e)
        {
            ((ImageExpander)((Border)sender).Child).IsExpanded ^= true;
        }
    }
}
