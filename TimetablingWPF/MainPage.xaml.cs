using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
using Humanizer;

namespace TimetablingWPF
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        public MainPage()
        {
            Application.Current.MainWindow.WindowState = WindowState.Maximized;
            Application.Current.MainWindow.ResizeMode = ResizeMode.CanResize;
            Application.Current.MainWindow.SizeToContent = SizeToContent.Manual;
            InitializeComponent();

            void MoveWindow()
            {
                Vector vector = Mouse.GetPosition(tcMainTabControl) - DraggingTab.Item2;
                Window.GetWindow(tcMainTabControl).Left += vector.X;
                Window.GetWindow(tcMainTabControl).Top += vector.Y;
            }
            void ReleaseMouse()
            {
                DraggingTab = null;
                tcMainTabControl.ReleaseMouseCapture();
            }
            void MouseMoveDragTab(object sender, MouseEventArgs e)
            {
                if (DraggingTab == null || !(e.LeftButton == MouseButtonState.Pressed))
                {
                    return;
                }
                if (tcMainTabControl.Items.Count == 1)
                {
                    MoveWindow();
                    return;
                }
                if (e.GetPosition(tcMainTabControl).Y >
                    DraggingTab.Item1.TransformToAncestor(TabToContent(DraggingTab.Item1.Parent).MainPage.tcMainTabControl).Transform(new Point(0, 0)).Y + DraggingTab.Item1.ActualHeight)
                {
                    MouseLeaveDragTab(sender, e);
                }
            }
            void MouseLeaveDragTab(object sender, MouseEventArgs e)
            {
                if (DraggingTab == null || !(e.LeftButton == MouseButtonState.Pressed))
                {
                    return;
                }
                if (tcMainTabControl.Items.Count == 1)
                {
                    MoveWindow();
                    return;
                }
                TabItem tab = (TabItem)DraggingTab.Item1.Parent;
                Vector localToMouse = DraggingTab.Item2.ToVector();
                Vector screenToMouse = DraggingTab.Item1.PointToScreen(DraggingTab.Item2).ToVector();
                tcMainTabControl.Items.Remove(tab);
                TabItem newTab = new TabItem();
                GenericHelpers.MoveElementProperties(tab, newTab, new string[] { "Header", "Content" });

                //void newWindow(object sender2, EventArgs e2)
                //{
                MainWindow window = new MainWindow();
                MainPage mainPage = window.GetMainPage();
                TabToContent(newTab).MainPage = mainPage;
                mainPage.tcMainTabControl.Items.Add(newTab);
                window.Show();
                Vector windowToLocal = ((FrameworkElement)newTab.Header).PointToScreen(new Point()).ToVector() - window.VectorPos().VectorFromScreen();
                Vector screenToWindow = screenToMouse - localToMouse - windowToLocal;
                window.SetPos((screenToMouse-localToMouse).VectorToScreen());

                window.SetPos(screenToWindow.VectorToScreen());
                mainPage.DraggingTab = new Tuple<FrameworkElement, Point>((FrameworkElement)newTab.Header, Mouse.GetPosition((FrameworkElement)newTab.Header));
                ReleaseMouse();
                mainPage.tcMainTabControl.CaptureMouse();
                //}
            }
            void MouseUp(object sender, MouseButtonEventArgs e)
            {
                ReleaseMouse();
            }
            void MouseEnter(object sender, MouseEventArgs e)
            {
                if (tcMainTabControl.IsMouseCaptureWithin)
                {

                }
            }
            tcMainTabControl.MouseLeftButtonUp += MouseUp;
            tcMainTabControl.MouseMove += MouseMoveDragTab;
            tcMainTabControl.MouseLeave += MouseLeaveDragTab;
            tcMainTabControl.MouseEnter += MouseEnter;
        }

        public Tuple<FrameworkElement, Point> DraggingTab;

        public void NewTab(object page, string title, bool focus = true, bool draggable = true)
        {
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            TextBlock tb = new TextBlock() { Text = title };
            Grid.SetColumn(tb, 0);
            grid.Children.Add(tb);
            Button button = new Button() { Content = new TextBlock() { Text = "x" } };
            button.Click += delegate (object sender, RoutedEventArgs e) { ((ITab)page).Cancel(); };
            Grid.SetColumn(button, 1);
            grid.Children.Add(button);
            TabItem newTab = new TabItem()
            {
                Content = page,
                Header = grid
            };
            tcMainTabControl.Items.Add(newTab);
            if (draggable)
            {
                void TabHeaderMouseDown(object sender, MouseButtonEventArgs e)
                {
                    FrameworkElement element = (FrameworkElement)sender;
                    ITab content = TabToContent(element.Parent);
                    content.MainPage.DraggingTab = new Tuple<FrameworkElement, Point>(element, e.GetPosition(element));
                    content.MainPage.tcMainTabControl.CaptureMouse();
                   // TabToContent(((FrameworkElement)sender).Parent).MainPage.tcMainTabControl.CaptureMouse();
                }

                grid.MouseLeftButtonDown += TabHeaderMouseDown;
            }

            if (focus) { tcMainTabControl.SelectedItem = newTab; }
        }

        public void CloseTab(object page)
        {
            foreach (TabItem tab in tcMainTabControl.Items)
            {
                if (tab.Content == page)
                {
                    tcMainTabControl.Items.Remove(tab);
                    if (tcMainTabControl.Items.Count == 0 && Application.Current.Windows.Count > 1)
                    {
                        Window.GetWindow(this).Close();
                    }
                    return;
                }
            }
            throw new System.ArgumentException($"Page {page} of type {page.GetType().Name} does not exist in the tab list");
        }
        public static ITab TabToContent(object tab)
        {
            return (ITab)((TabItem)tab).Content;
        }

        public void NewDataClassTab(Type type)
        {
            tcMainTabControl.Items.Add(new DataClassTabItem(this, type) { Header = type.Name });
        }

        public void CloseDataClassTab(Type type)
        {
            for (int i=0; i<tcMainTabControl.Items.Count; i++)
            {
                DataClassTabItem tabItem = tcMainTabControl.Items[i] as DataClassTabItem;
                if (tabItem != null && tabItem.DataType == type)
                {
                    tcMainTabControl.Items.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public interface ITab
    {
        MainPage MainPage { get; set; }
        void Cancel();
    }

    public class ListFormatter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return "N/A";
            }
            if (((IList)value).Count == 0)
            {
                return "None";
            }
            IEnumerable<object> enumerable = ((IList)value).Cast<object>();
            return string.Join(", ", enumerable);
        }
        public object ConvertBack(object value, Type targetType, object paramter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
