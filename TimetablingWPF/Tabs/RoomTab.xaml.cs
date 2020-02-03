using Humanizer;
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
using static TimetablingWPF.DataHelpers;


namespace TimetablingWPF
{
    /// <summary>
    /// Interaction logic for TeacherTab.xaml
    /// </summary>
    public partial class RoomTab : TabBase
    {
        public RoomTab(Room room, CommandType commandType)
        {
            InitializeComponent();

            ErrManager = new ErrorManager(spErrors);
            CommandType = commandType;
            OriginalRoom = room;
            Room = commandType == CommandType.@new ? room : (Room)room.Clone();
            Room.Freeze();
            tbTitle.Text = "Create a new Room";
            txName.Text = room.Name;
            txName.SelectionStart = txName.Text.Length;
            cmbxGroups.ItemsSource = GetDataContainer().Groups;
            ilGroups.ItemsSource = Room.Groups;
            ilGroups.ListenToCollection(OriginalRoom.Groups);

            HAS_NO_NAME = GenericHelpers.GenerateNameError(ErrManager, txName, "Room");
        }

        private void GroupButtonClick(object sender, RoutedEventArgs e)
        {

            Group group = cmbxGroups.GetObject<Group>();
            if (group != null && !Room.Groups.Contains(group))
            {
                group.Commit();
                cmbxGroups.SelectedItem = group;
                Room.Groups.Add(group);
            }
        }

        private void RemoveGroup(object sender, RoutedEventArgs e)
        {
            StackPanel sp = (StackPanel)((FrameworkElement)sender).Tag;
            Group group = (Group)sp.Tag;
            Room.Groups.Remove(group);
        }

        private readonly Room Room;
        private readonly Room OriginalRoom;
        private readonly ErrorContainer HAS_NO_NAME;
        private readonly ErrorManager ErrManager;
        private readonly CommandType CommandType;

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (Cancel())
            {
                MainPage.CloseTab(this);
            }
        }

        private void Confirm(object sender, RoutedEventArgs e)
        {
            HAS_NO_NAME.UpdateError();
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
            Room.Critical = checkCritical.IsChecked ?? false;
            Room.Quantity = iupdown.Value ?? 0;
            Room.Unfreeze();
            if (CommandType == CommandType.edit) {
                OriginalRoom.UpdateWithClone(Room);
            } else
            {
                Room.Commit();
            }
            MainPage.CloseTab(this);
            
        }
    }
}
