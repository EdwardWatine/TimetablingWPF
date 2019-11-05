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

namespace TimetablingWPF
{
    /// <summary>
    /// Interaction logic for TeacherTab.xaml
    /// </summary>
    public partial class GroupTab : Grid, ITab
    {
        public GroupTab(Group group, MainPage mainPage, CommandType commandType)
        {
            InitializeComponent();
            MainPage = mainPage;
            ErrManager = new ErrorManager(spErrors);
            CommandType = commandType;
            OriginalGroup = group;
            Group = commandType == CommandType.@new ? group : (Group)group.Clone();
            Group.Freeze();
            tbTitle.Text = "Create a new Group";
            txName.Text = group.Name;
            txName.SelectionStart = txName.Text.Length;
            cmbxSubject.ItemsSource = (IEnumerable<Subject>)Application.Current.Properties[Subject.ListName];
            cmbxRoom.ItemsSource = (IEnumerable<Form>)Application.Current.Properties[Form.ListName];

            HAS_NO_NAME = GenericHelpers.GenerateNameError(ErrManager, txName, "Group");
        }

        private void SubjectButtonClick(object sender, RoutedEventArgs e)
        {

            Subject subject = cmbxSubject.GetObject<Subject>();
            if (subject != null && !Group.Subjects.Contains(subject))
            {
                subject.Commit();
                AddSubject(subject);
                cmbxSubject.SelectedItem = subject;
                Group.Subjects.Add(subject);
            }
        }

        private void RoomButtonClick(object sender, RoutedEventArgs e)
        {
            Room room = (Room)cmbxRoom.SelectedItem;
            if (room != null && !Group.Rooms.Contains(room))
            {
                AddRoom(room);
                cmbxRoom.SelectedItem = room;
                Group.Rooms.Add(room);
            }
        }

        private void AddSubject(Subject subject)
        {            
            spSubjects.Children.Add(VerticalMenuItem(subject, RemoveSubject));
        }

        private void RemoveSubject(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            StackPanel sp = (StackPanel)element.Parent;
            Subject subject = (Subject)element.Tag;
            Group.Subjects.Remove(subject);
            spSubjects.Children.Remove(sp);
        }
        private void AddRoom(Room subject)
        {            
            spRooms.Children.Add(VerticalMenuItem(subject, RemoveRoom));
        }

        private void RemoveRoom(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            StackPanel sp = (StackPanel)element.Parent;
            Room room = (Room)element.Tag;
            Group.Rooms.Remove(room);
            spRooms.Children.Remove(sp);
        }

        private readonly Group Group;
        private readonly Group OriginalGroup;
        private readonly ErrorContainer HAS_NO_NAME;
        private readonly ErrorManager ErrManager;
        public MainPage MainPage { get; set; }
        private readonly CommandType CommandType;

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
            Group.Name = txName.Text;
            Group.Unfreeze();
            if (CommandType == CommandType.edit) {
                OriginalGroup.Recommit(Group);
            } else
            {
                Group.Commit();
            }
            
            MainPage.CloseTab(this);
        }
    }
}
