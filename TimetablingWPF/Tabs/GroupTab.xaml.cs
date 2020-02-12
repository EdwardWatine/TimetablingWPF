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
    public partial class GroupTab : TabBase
    {
        public GroupTab(Group group, CommandType commandType)
        {
            InitializeComponent();

            ErrManager = new ErrorManager(spErrors);
            CommandType = commandType;
            OriginalGroup = group;
            Group = commandType == CommandType.@new ? group : (Group)group.Clone();
            Group.Freeze();
            tbTitle.Text = "Create a new Group";
            txName.Text = group.Name;
            txName.SelectionStart = txName.Text.Length;
            cmbxSubject.ItemsSource = GetDataContainer().Subjects;
            cmbxRoom.ItemsSource = GetDataContainer().Rooms;
            ilRooms.ItemsSource = Group.Rooms;
            
            ilSubjects.ItemsSource = Group.Subjects;
            if (commandType != CommandType.@new)
            {
                ilSubjects.ListenToCollection(OriginalGroup.Subjects);
                ilRooms.ListenToCollection(OriginalGroup.Rooms);
            }
            

            HAS_NO_NAME = GenericHelpers.GenerateNameError(ErrManager, txName, "Group");
        }

        private void SubjectButtonClick(object sender, RoutedEventArgs e)
        {

            Subject subject = cmbxSubject.GetObject<Subject>();
            if (subject != null && !Group.Subjects.Contains(subject))
            {
                subject.Commit();
                cmbxSubject.SelectedItem = subject;
                Group.Subjects.Add(subject);
            }
        }

        private void RoomButtonClick(object sender, RoutedEventArgs e)
        {
            Room room = (Room)cmbxRoom.SelectedItem;
            if (room != null && !Group.Rooms.Contains(room))
            {
                cmbxRoom.SelectedItem = room;
                Group.Rooms.Add(room);
            }
        }

        private readonly Group Group;
        private readonly Group OriginalGroup;
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
            Group.Name = txName.Text.Trim();
            Group.Unfreeze();
            if (CommandType == CommandType.edit) {
                OriginalGroup.UpdateWithClone(Group);
            } else
            {
                Group.Commit();
            }
            MainPage.CloseTab(this);
            
        }
    }
}
