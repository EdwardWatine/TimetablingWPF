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
            animDark = (ColorAnimation)Resources["animDark"];
            animLight = (ColorAnimation)Resources["animLight"];
            animWhite = (ColorAnimation)Resources["animWhite"];
            icYear.ItemsSource = App.Data.YearGroups;
        }

        private Border selected;
        private Border bottom;
        private readonly ColorAnimation animDark;
        private readonly ColorAnimation animLight;
        private readonly ColorAnimation animWhite;

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
        }
        private void SetBorder(Thickness th)
        {
            if (IsLastInContainer(selected))
            {
                th.Bottom = 0;
            }
            else
            {
                th.Bottom = 1;
            }
            selected.BorderThickness = th;
        }
        private void FixBorder(int bottom_t)
        {
            Thickness t = bottom.BorderThickness;
            bottom.BorderThickness = new Thickness(t.Left, t.Top, t.Right, bottom_t);
        }
        private void SetSelected(Border border)
        {
            selected = border;
            ((SolidColorBrush)border.Background).BeginAnimation(SolidColorBrush.ColorProperty, animWhite);
            SetBorder(new Thickness(0, 0, 0, 1));

        }
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (selected != sender)
            {
                ToNormal(selected);
                SetBorder(new Thickness(0, 0, 1, 1));
                SetSelected((Border)sender);
            }
        }
        private static bool IsLastInContainer(Border b)
        {
            return App.Data.YearGroups.IndexOf((Year)b.DataContext) == App.Data.YearGroups.Count - 1;
        }
        private void Border_Loaded(object sender, RoutedEventArgs e)
        {
            Border b = (Border)sender;
            if (selected == null)
            {
                SetSelected(b);
            }
            if (IsLastInContainer(b))
            {
                if (bottom != null)
                {
                    FixBorder(1);
                }
                bottom = b;
                FixBorder(0);
            }
        }
    }
}
