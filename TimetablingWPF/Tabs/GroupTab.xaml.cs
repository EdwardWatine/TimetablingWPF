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
            Group = (Group)group.Clone();
            tbTitle.Text = "Create a new Group";
            txName.Text = group.Name;
            txName.SelectionStart = txName.Text.Length;
            cmbxSubject.ItemsSource = (IEnumerable<Subject>)Application.Current.Properties[Subject.ListName];
            cmbxRoom.ItemsSource = (IEnumerable<Form>)Application.Current.Properties[Form.ListName];


        }

        private void SubjectButtonClick(object sender, RoutedEventArgs e)
        {
            
            Subject subject = (Subject)cmbxSubject.SelectedItem;
            if (subject == null)
            {
                if (string.IsNullOrWhiteSpace(cmbxSubject.Text))
                {
                    return;
                }
                subject = new Subject() { Name = cmbxSubject.Text.Trim() };
                subject.Commit();
            }
            else
            {
                if (Group.Subjects.Contains(subject))
                {
                    return;
                }
            }
            AddSubject(subject);
            cmbxSubject.SelectedItem = subject;
            Group.Subjects.Add(subject);
        }

        private void RoomButtonClick(object sender, RoutedEventArgs e)
        {
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

        private readonly Group Group;
        private readonly Group OriginalGroup;
        public MainPage MainPage { get; set; }
        private readonly TimetableStructure Structure = (TimetableStructure)Application.Current.Properties[TimetableStructure.ListName];
        private readonly ErrorManager ErrManager;
        private CommandType CommandType;

        private void CmbxSubjects_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                SubjectButtonClick(null, null);
            }
            if (e.Key == Key.Escape)
            {
                cmbxSubject.SelectedItem = null;
                cmbxSubject.Text = "";
                Keyboard.ClearFocus();
            }
        }

        private void SubjectButtonClick(object sender, SelectionChangedEventArgs e)
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
            Group.Name = txName.Text;

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
