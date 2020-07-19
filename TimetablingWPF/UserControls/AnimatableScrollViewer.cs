using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TimetablingWPF
{
    public class AnimatableScrollViewer : ScrollViewer
    {
        public AnimatableScrollViewer()
        {
            ScrollChanged += UpdateScroll;
            UpdateScroll(null, null);
        }
        private void UpdateScroll(object sender, ScrollChangedEventArgs e)
        {
            SetValue(AnimatableVerticalOffsetProperty, VerticalOffset);
            SetValue(AnimatableHorizontalOffsetProperty, HorizontalOffset);
        }

        private static void AnimateVertically(object sender, DependencyPropertyChangedEventArgs e)
        {
            AnimatableScrollViewer _this = (AnimatableScrollViewer)sender;
            _this.ScrollToVerticalOffset((double)e.NewValue);
        }
        private static void AnimateHorizontally(object sender, DependencyPropertyChangedEventArgs e)
        {
            AnimatableScrollViewer _this = (AnimatableScrollViewer)sender;
            _this.ScrollToHorizontalOffset((double)e.NewValue);
        }

        public static readonly DependencyProperty AnimatableVerticalOffsetProperty = DependencyProperty.Register("AnimatableVerticalOffsetProperty", typeof(double), typeof(AnimatableScrollViewer),
            new PropertyMetadata(0.0, new PropertyChangedCallback(AnimateVertically)));
        public static readonly DependencyProperty AnimatableHorizontalOffsetProperty = DependencyProperty.Register("AnimatableHorizontalOffsetProperty", typeof(double), typeof(AnimatableScrollViewer),
            new PropertyMetadata(0.0, new PropertyChangedCallback(AnimateHorizontally)));
    }
}
