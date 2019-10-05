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

namespace TimetablingWPF
{
    /// <summary>
    /// Interaction logic for TeacherTab.xaml
    /// </summary>
    public partial class SubjectTab : Page, ITab
    {
        public SubjectTab(Subject subject, MainPage mainPage, CommandType commandType)
        {
            InitializeComponent();
            MainPage = mainPage;
            ErrManager = new ErrorManager(spErrors);
            CommandType = commandType;
            OriginalSubject = subject;
            Subject = commandType == CommandType.@new ? subject : (Subject)subject.Clone();
            tbTitle.Text = "Create a new Teacher";
            txName.Text = subject.Name;
            txName.SelectionStart = txName.Text.Length;
            cmbxRooms.ItemsSource = (IEnumerable<Room>)Application.Current.Properties[Room.ListName];
            cmbxTeachers.ItemsSource = (IEnumerable<Teacher>)Application.Current.Properties[Teacher.ListName];
            //Errors

            foreach (Room room in Subject.Rooms)
            {
                AddRoom(room);
            }
            foreach (Teacher teacher in Subject.Teachers)
            {
                AddTeacher(teacher);
            }
        }

        private void RoomButtonClick(object sender, RoutedEventArgs e)
        {
            
            Room room = (Room)cmbxRooms.SelectedItem;
            if (room == null)
            {
                return;
            }
            AddRoom(room);
            cmbxRooms.SelectedItem = room;
            Subject.Rooms.Add(room);
        }

        private void TeacherButtonClick(object sender, RoutedEventArgs e)
        {
            Teacher teacher = (Teacher)cmbxTeachers.SelectedItem;
            if (teacher == null)
            {
                return;
            }
            AddTeacher(teacher);
            Subject.Teachers.Add(teacher);
        }

        private void AddRoom(Room room)
        {            
            spRooms.Children.Add(Utility.VerticalMenuItem(room, RemoveRoom));
        }

        private void RemoveRoom(object sender, RoutedEventArgs e)
        {
            StackPanel sp = (StackPanel)((FrameworkElement)sender).Tag;
            Room room = (Room)sp.Tag;
            Subject.Rooms.Remove(room);
            spRooms.Children.Remove(sp);
        }

        private void AddTeacher(Teacher teacher)
        {
            spTeachers.Children.Add(Utility.VerticalMenuItem(teacher, RemoveTeacher));
        }

        private void RemoveTeacher(object sender, RoutedEventArgs e)
        {
            StackPanel sp = (StackPanel)((FrameworkElement)sender).Tag;
            Teacher teacher = (Teacher)sp.Tag;
            Subject.Teachers.Remove(teacher);
            spTeachers.Children.Remove(sp);
        }
        private readonly Subject Subject;
        private readonly Subject OriginalSubject;
        private readonly TimetableStructure Structure = (TimetableStructure)Application.Current.Properties[TimetableStructure.ListName];
        private readonly Error HAS_NO_PERIODS = new Error("Teacher has no periods", ErrorType.Warning);
        private readonly Error NOT_ENOUGH_PERIODS = new Error("Teacher does not have enough free periods", ErrorType.Error);
        private readonly Error HAS_EMPTY_NAME = new Error("Teacher does not have a name", ErrorType.Error);
        private readonly ErrorManager ErrManager;
        public MainPage MainPage { get; set; }
        private CommandType CommandType;

        private void TxNameChanged(object sender, TextChangedEventArgs e)
        {
            ErrManager.UpdateError(HAS_EMPTY_NAME, string.IsNullOrWhiteSpace(txName.Text));
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
            ErrManager.UpdateError(HAS_EMPTY_NAME, string.IsNullOrWhiteSpace(txName.Text));
            if (ErrManager.GetNumErrors() > 0)
            {
                Utility.ShowErrorBox("Please fix all errors!");
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
            Subject.Name = txName.Text;

            if (CommandType == CommandType.edit) {
                OriginalSubject.Recommit(Subject);
            } else
            {
                Subject.Commit();
            }
            
            MainPage.CloseTab(this);
        }
    }
}
