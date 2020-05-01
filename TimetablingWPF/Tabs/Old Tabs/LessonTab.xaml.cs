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
    /// Interaction logic for AssignmentTab.xaml
    /// </summary>
    public partial class LessonTab : TabBase
    {
        public LessonTab(Lesson lesson, CommandType commandType)
        {
            InitializeComponent();

            ErrManager = new ErrorManager(spErrors);
            CommandType = commandType;
            OriginalLesson = lesson;
            Lesson = commandType == CommandType.@new ? lesson : (Lesson)lesson.Clone();
            Lesson.Freeze();
            tbTitle.Text = "Create a new Lesson";
            txName.Text = lesson.Name;
            txName.SelectionStart = txName.Text.Length;

            DataContainer data = GetDataContainer();
            cmbxSubject.ItemsSource = data.Subjects;
            cmbxAssignmentTeacher.ItemsSource = data.Teachers;
            cmbxForm.ItemsSource = data.Forms;

            ilAssignments.ItemsSource = Lesson.Assignments;
            
            ilForms.ItemsSource = Lesson.Forms;
            if (commandType != CommandType.@new)
            {
                ilForms.ListenToCollection(OriginalLesson.Forms);
                ilAssignments.ListenToCollection(OriginalLesson.Assignments);
            }
            HAS_NO_NAME = GenericHelpers.GenerateNameError(txName, "Lesson");
            HAS_NO_SUBJECT = new ErrorContainer((e) => cmbxSubject.SelectedItem == null, (e) => "No subject has been selected.", ErrorType.Error);
        }
        private readonly Lesson Lesson;
        private readonly Lesson OriginalLesson;
        private readonly ErrorContainer HAS_NO_SUBJECT;
        private readonly ErrorContainer HAS_NO_NAME;
        private readonly ErrorManager ErrManager;
        private readonly CommandType CommandType;

        private void AssignmentButtonClick(object sender, RoutedEventArgs e)
        {
            Teacher teacher = (Teacher)cmbxAssignmentTeacher.SelectedItem;
            int? lessons = iupdown.Value;
            if (teacher == null || lessons == null)
            {
                return;
            }
            Assignment old = Lesson.Assignments.Where(a => a.Teacher == teacher).SingleOrDefault();
            if (old != null)
            {
                if (old.LessonCount == lessons) { return; }
                Lesson.Assignments.Remove(old);
            }
            Assignment assignment = new Assignment(teacher, Lesson, (int)lessons);
            Lesson.Assignments.Add(assignment);
        }

        private void FormButtonClick(object sender, RoutedEventArgs e)
        {
            Form form = (Form)cmbxForm.SelectedItem;
            if (form != null && !Lesson.Forms.Contains(form))
            {
                cmbxForm.SelectedItem = form;
                Lesson.Forms.Add(form);
            }
        }

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
            HAS_NO_SUBJECT.UpdateError();
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
            Lesson.Name = txName.Text.Trim();
            Lesson.Subject = (Subject)cmbxSubject.SelectedItem;
            Lesson.LessonLength = (int)iupdownLength.Value;
            Lesson.LessonsPerCycle = (int)iupdownPerCycle.Value;
            Lesson.Unfreeze();
            if (CommandType == CommandType.edit) {
                OriginalLesson.UpdateWithClone(Lesson);
            } else
            {
                Lesson.Commit();
            }
            MainPage.CloseTab(this);
            
        }
    }
}
