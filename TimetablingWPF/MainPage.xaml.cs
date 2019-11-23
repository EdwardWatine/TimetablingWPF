using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Dragablz;
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
            InitializeComponent();
            tcMainTabControl.ClosingItemCallback = CloseTab;
        }

        public Stack<TabItem> TabHistory { get; } = new Stack<TabItem>();

        public void NewTab(object page, string title, bool focus = true)
        {
            TabItem newTab = new TabItem()
            {
                Content = page,
                Header = new TextBlock() { Text = title }
            };
            tcMainTabControl.Items.Add(newTab);
            if (focus)
            { 
                tcMainTabControl.SelectedItem = newTab;
                TabHistory.Push(newTab);
            }
        }

        public static void CloseTab(ItemActionCallbackArgs<TabablzControl> args)
        {
            if (!TabToContent(args.DragablzItem.Content).Cancel())
            {
                args.Cancel();
            }
        }
        public static ITab TabToContent(object tab)
        {
            if (tab is DataClassTabItem)
            {
                return (ITab)tab;
            }
            return (ITab)((TabItem)tab).Content;
        }

        public void NewDataSetTab(Type type)
        {
            TextBlock header = new TextBlock() { Text = type.Name.Pluralize() };
            header.MouseLeftButtonUp += ManualChange;
            DataClassTabItem dataTab = new DataClassTabItem(this, type) { Header = header };
            tcMainTabControl.Items.Add(dataTab);
            if (tcMainTabControl.Items.Count == 1)
            {
                TabHistory.Push((TabItem)tcMainTabControl.Items[0]);
            }
        }

        public void CloseDataSetTab(Type type)
        {
            for (int i=0; i<tcMainTabControl.Items.Count; i++)
            {
                if (tcMainTabControl.Items[i] is DataClassTabItem tabItem && tabItem.DataType == type)
                {
                    tcMainTabControl.Items.RemoveAt(i);
                    if (TabHistory.FirstOrDefault() == tabItem)
                    {
                        TabHistory.Pop();
                        tcMainTabControl.SelectedItem = TabHistory.FirstOrDefault();
                    }

                    return;
                }
            }
        }

        private void ManualChange(object sender, MouseButtonEventArgs e)
        {
            if (tcMainTabControl.SelectedItem != TabHistory.FirstOrDefault())
            {
                TabHistory.Clear();
                TabHistory.Push((TabItem)tcMainTabControl.SelectedItem);
            }
        }
    }

    public class InterTabClient : IInterTabClient
    {
        public INewTabHost<Window> GetNewHost(IInterTabClient interTabClient, object partition, TabablzControl source)
        {
            MainWindow window = new MainWindow(fullscreen: false);
            MainPage mainPage = window.GetMainPage();
            return new NewTabHost<Window>(window, mainPage.tcMainTabControl);
        }

        public TabEmptiedResponse TabEmptiedHandler(TabablzControl tabControl, Window window)
        {
            return Application.Current.Windows.Count == 1 ? TabEmptiedResponse.DoNothing : TabEmptiedResponse.CloseWindowOrLayoutBranch;
        }
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
            IEnumerable<object> enumerable = ((IEnumerable)value).Cast<object>();
            return GenericHelpers.FormatEnumerable(enumerable);
        }
        public object ConvertBack(object value, Type targetType, object paramter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
