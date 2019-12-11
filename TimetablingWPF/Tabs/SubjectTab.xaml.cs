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
    public partial class SubjectTab : TabItem, ITab
    {
        public SubjectTab(Subject subject, MainPage mainPage, CommandType commandType)
        {
            InitializeComponent();
            MainPage = mainPage;
            ErrManager = new ErrorManager(spErrors);
            CommandType = commandType;
            OriginalSubject = subject;
            Subject = commandType == CommandType.@new ? subject : (Subject)subject.Clone();
            Subject.Freeze();
            tbTitle.Text = "Create a new Subject";
            txName.Text = subject.Name;
            txName.SelectionStart = txName.Text.Length;
            cmbxGroups.ItemsSource = GetData<Group>();
            cmbxTeachers.ItemsSource = GetData<Teacher>();
            //Errors

            HAS_NO_NAME = GenericHelpers.GenerateNameError(ErrManager, txName, "Subject");
            ilGroups.ItemsSource = Subject.Groups;
            ilGroups.ListenToCollection(OriginalSubject.Groups);
            ilTeachers.ItemsSource = Subject.Teachers;
            ilTeachers.ListenToCollection(OriginalSubject.Teachers);
        }

        private void GroupButtonClick(object sender, RoutedEventArgs e)
        {

            Group group = cmbxGroups.GetObject<Group>();
            if (group != null && !Subject.Groups.Contains(group))
            {
                group.Commit();
                cmbxGroups.SelectedItem = group;
                Subject.Groups.Add(group);
            }
        }

        private void TeacherButtonClick(object sender, RoutedEventArgs e)
        {
            Teacher teacher = cmbxTeachers.GetObject<Teacher>();
            if (teacher != null && !Subject.Teachers.Contains(teacher))
            {
                teacher.Commit();
                cmbxTeachers.SelectedItem = teacher;
                Subject.Teachers.Add(teacher);
            }
        }

        private readonly Subject Subject;
        private readonly Subject OriginalSubject;
        private readonly ErrorContainer HAS_NO_NAME;
        private readonly ErrorManager ErrManager;
        public MainPage MainPage { get; set; }
        private readonly CommandType CommandType;

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (Cancel()){				MainPage.CloseTab(this);			}
        }

        public bool Cancel()
        {
            return (System.Windows.MessageBox.Show("Are you sure you want to discard your changes?",
                "Discard changes?", MessageBoxButton.YesNo) == MessageBoxResult.Yes);
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
            Subject.Name = txName.Text;
            Subject.Unfreeze();
            if (CommandType == CommandType.edit) {
                OriginalSubject.UpdateWithClone(Subject);
            } else
            {
                Subject.Commit();
            }
            MainPage.CloseTab(this);
            
        }
    }
}
