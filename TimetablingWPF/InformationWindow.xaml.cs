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
using System.Reflection;
using System.Collections;

namespace TimetablingWPF
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class InformationWindow : Window
    {
        public InformationWindow(BaseDataClass item)
        {
            InitializeComponent();
            Populate(item);
            Title = $"Information for {item.GetType().Name} '{item.Name}'";
        }
        private void Populate(BaseDataClass item)
        {
            StackPanel GenerateTextPanel(string label, FrameworkElement data)
            {
                StackPanel sp = new StackPanel() { Orientation = Orientation.Horizontal };
                sp.Children.Add(new TextBlock()
                {
                    Text = label,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 3, 0)
                });
                data.Margin = new Thickness(0, 0, 10, 0);
                sp.Children.Add(data);
                return sp;
            }
            StackPanel GenerateListPanel(string label, IList data)
            {
                StackPanel sp = new StackPanel() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
                sp.Children.Add(new TextBlock()
                {
                    Text = label,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 3, 0)
                });
                sp.Children.Add(
                    new ScrollViewer()
                    {
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                        Content = new ItemsControl()
                        {
                            ItemsSource = data,
                            BorderThickness = new Thickness(0)
                        }
                    });
                return sp;
            }
            wpTop.Children.Add(GenerateTextPanel("Name:", new TextBlock() { Text = item.Name }));
            foreach (CustomPropertyInfo prop in BaseDataClass.ExposedProperties[item.GetType()])
            {
                if (prop.PropertyInfo.PropertyType.IsInterface<IList>())
                {
                    if (!(prop.PropertyInfo.PropertyType == typeof(ObservableCollection<TimetableSlot>)))
                    {
                        IList data = (IList)prop.PropertyInfo.GetValue(item);
                        StackPanel sp;
                        if (data.Count == 0)
                        {
                            sp = GenerateTextPanel(prop.Alias + ":", new TextBlock() { Text = "None" });
                        }
                        else
                        {
                            sp = GenerateListPanel(prop.Alias + ":", data);
                        }
                        sp.HorizontalAlignment = HorizontalAlignment.Center;
                        gdBottom.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                        Grid.SetColumn(sp, gdBottom.ColumnDefinitions.Count - 1);
                        gdBottom.Children.Add(sp);
                    }
                }
                else
                {
                    wpTop.Children.Add(GenerateTextPanel(prop.Alias + ":", new TextBlock()
                    {
                        Text = prop.Display(prop.PropertyInfo.GetValue(item))
                    }));
                }
            }
        }
    }
}
