﻿using Humanizer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using static TimetablingWPF.FileHelpers;

namespace TimetablingWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        public MainWindow(bool loadDataTabs = false, bool fullscreen = true)
        {
            WindowState = fullscreen ? WindowState.Maximized : WindowState.Normal;
            InitializeComponent();
            Title = $"Timetabler - {Application.Current.Properties["CURRENT_FILE_PATH"]}";
            //Subject Science = new Subject("Science");

            //Room sgroup = new Room("Science Rooms", 5);
            MainPage mp = new MainPage();
            if (loadDataTabs)
            {
                foreach (Type type in (Type[])Application.Current.Properties["USER_TYPES"])
                {
                    mp.NewDataSetTab(type);
                }
            }
            Content = mp;
            return;

        }

        public MainPage GetMainPage()
        {
            return (MainPage)Content;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (TabItem tab in GetMainPage().tcMainTabControl.Items)
            {
                if (!(tab is DataClassTabItem))
                {
                    if (VisualHelpers.ShowWarningBox("Data has not been saved. Close Window?", "Data Unsaved") == MessageBoxResult.Cancel)
                    {
                        e.Cancel = true;
                    }
                }
                return;
            }
        }
    }
}
