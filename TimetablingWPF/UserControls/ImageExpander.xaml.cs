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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TimetablingWPF
{
    /// <summary>
    /// Interaction logic for ImageExpander.xaml
    /// </summary>
    public partial class ImageExpander : UCBases.ImageExpanderBase
    {
        public ImageExpander()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
namespace TimetablingWPF.UCBases
{
    public class ImageExpanderBase : Image
    {
        public static readonly RoutedEvent ExpandedEvent = EventManager.RegisterRoutedEvent(nameof(Expanded), RoutingStrategy.Direct,
            typeof(RoutedEventHandler), typeof(ImageExpander));
        public event RoutedEventHandler Expanded
        {
            add { AddHandler(ExpandedEvent, value); }
            remove { RemoveHandler(ExpandedEvent, value); }
        }
        public static readonly RoutedEvent CollapsedEvent = EventManager.RegisterRoutedEvent(nameof(Collapsed), RoutingStrategy.Direct,
            typeof(RoutedEventHandler), typeof(ImageExpander));
        public event RoutedEventHandler Collapsed
        {
            add { AddHandler(CollapsedEvent, value); }
            remove { RemoveHandler(CollapsedEvent, value); }
        }
        public static readonly DependencyProperty ExpandedProperty = DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(ImageExpanderBase));
        public bool IsExpanded
        {
            get => (bool)GetValue(ExpandedProperty);
            set
            {
                if (value ^ IsExpanded)
                {
                    SetValue(ExpandedProperty, value);
                    RaiseEvent(new RoutedEventArgs(value ? ExpandedEvent : CollapsedEvent));
                }
            }
        }
        public static readonly DependencyProperty DurationProperty = DependencyProperty.Register(nameof(Duration), typeof(Duration), typeof(ImageExpanderBase));
        public Duration Duration
        {
            get => (Duration)GetValue(DurationProperty);
            set => SetValue(DurationProperty, value);
        }
    }
}
