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

        public void NewTab(TabItem tabItem, string title, bool focus = true)
        {
            tabItem.Header = title;
            tcMainTabControl.Items.Add(tabItem);
            if (focus)
            { 
                tcMainTabControl.SelectedItem = tabItem;
                TabHistory.Push(tabItem);
            }
        }
        public void CloseTab(TabItem tabItem)
        {
            tcMainTabControl.Items.Remove(tabItem);
        }
        private static void CloseTab(ItemActionCallbackArgs<TabablzControl> args)
        {
            if (!((TabBase)args.DragablzItem.Content).Cancel())
            {
                args.Cancel();
            }
        }

        public void NewDataSetTab(Type type)
        {
            TextBlock header = new TextBlock() { Text = type.Name.Pluralize() };
            header.MouseLeftButtonUp += ManualChange;
            DataClassTabItem dataTab = new DataClassTabItem(type) { Header = header };
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
            return TabEmptiedResponse.DoNothing;
        }
    }
}
