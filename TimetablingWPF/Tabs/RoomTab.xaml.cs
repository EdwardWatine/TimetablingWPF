﻿using Humanizer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;
using static TimetablingWPF.VisualHelpers;

namespace TimetablingWPF
{
    /// <summary>
    /// Interaction logic for TeacherTab.xaml
    /// </summary>
    public partial class RoomTab : Grid, ITab
    {
        public RoomTab(Room room, MainPage mainPage, CommandType commandType)
        {
            InitializeComponent();
            MainPage = mainPage;
            ErrManager = new ErrorManager(spErrors);
            CommandType = commandType;
            OriginalRoom = room;
            Room = commandType == CommandType.@new ? room : (Room)room.Clone();
            tbTitle.Text = "Create a new Room";
            txName.Text = room.Name;
            txName.SelectionStart = txName.Text.Length;
            cmbxGroups.ItemsSource = (IEnumerable<Group>)Application.Current.Properties[Group.ListName];
            //Errors
        }

        private void GroupButtonClick(object sender, RoutedEventArgs e)
        {
            
            Group group = (Group)cmbxGroups.SelectedItem;
            if (group == null)
            {
                return;
            }
            AddGroup(group);
            cmbxGroups.SelectedItem = group;
            Room.Groups.Add(group);
        }


        private void AddGroup(Group group)
        {            
            spGroups.Children.Add(VerticalMenuItem(group, RemoveGroup));
        }

        private void RemoveGroup(object sender, RoutedEventArgs e)
        {
            StackPanel sp = (StackPanel)((FrameworkElement)sender).Tag;
            Group group = (Group)sp.Tag;
            Room.Groups.Remove(group);
            spGroups.Children.Remove(sp);
        }

        private readonly Room Room;
        private readonly Room OriginalRoom;
        private readonly ErrorManager ErrManager;
        public MainPage MainPage { get; set; }
        private CommandType CommandType;

        private void TxNameChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Cancel();
        }

        public void Cancel()
        {
            if (System.Windows.MessageBox.Show("Are you sure you want to discard your changes?",
                "Discard changes?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                MainPage.CloseTab(this);
            }
        }

        private void Confirm(object sender, RoutedEventArgs e)
        {
            if (ErrManager.GetNumErrors() > 0)
            {
                ShowErrorBox("Please fix all errors!");
                return;
            }
            if (ErrManager.GetNumWarnings() > 0)
            {
                if (System.Windows.MessageBox.Show("There are warnings. Do you want to continue?", "Warning", 
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                {
                    return;
                }
            }
            Room.Name = txName.Text;

            if (CommandType == CommandType.edit) {
                OriginalRoom.Recommit(Room);
            } else
            {
                Room.Commit();
            }
            
            MainPage.CloseTab(this);
        }
    }
}
