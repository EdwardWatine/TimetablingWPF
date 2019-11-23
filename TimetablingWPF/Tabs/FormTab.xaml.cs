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
    public partial class FormTab : Grid, ITab
    {
        public FormTab(Form form, MainPage mainPage, CommandType commandType)
        {
            InitializeComponent();
            MainPage = mainPage;
            ErrManager = new ErrorManager(spErrors);
            CommandType = commandType;
            OriginalForm = form;
            Form = commandType == CommandType.@new ? form : (Form)form.Clone();
            Form.Freeze();
            tbTitle.Text = "Create a new Form";
            txName.Text = form.Name;
            txName.SelectionStart = txName.Text.Length;
            cmbxLesson.ItemsSource = GetData<Lesson>();
            cmbxYear.ItemsSource = GetData<YearGroup>();
            HAS_NO_NAME = GenericHelpers.GenerateNameError(ErrManager, txName, "Form");
            HAS_NO_YEAR = new ErrorContainer(ErrManager, (e) => cmbxYear.SelectedItem == null, (e) => "No year group has been selected.", ErrorType.Error, false);
            cmbxYear.comboBox.SelectionChanged += delegate (object o, SelectionChangedEventArgs e) { HAS_NO_YEAR.UpdateError(); };
            //Errors
        }

        private void LessonButtonClick(object sender, RoutedEventArgs e)
        {

            Lesson lesson = (Lesson)cmbxLesson.SelectedItem;
            if (lesson != null && !Form.Lessons.Contains(lesson))
            {
                Form.Lessons.Add(lesson);
                AddLesson(lesson);
            }
        }

        private void AddLesson(Lesson lesson)
        {            
            spLessons.Children.Add(VerticalMenuItem(lesson, RemoveLesson));
        }

        private void RemoveLesson(object sender, RoutedEventArgs e)
        {
            StackPanel sp = (StackPanel)((FrameworkElement)sender).Tag;
            Lesson lesson = (Lesson)sp.Tag;
            spLessons.Children.Remove(sp);
            Form.Lessons.Remove(lesson);
        }

        private readonly Form Form;
        private readonly Form OriginalForm;
        private readonly ErrorContainer HAS_NO_YEAR;
        private readonly ErrorContainer HAS_NO_NAME;
        private readonly ErrorManager ErrManager;
        public MainPage MainPage { get; set; }
        private readonly CommandType CommandType;
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Cancel();
        }

        public bool Cancel()
        {
            return (System.Windows.MessageBox.Show("Are you sure you want to discard your changes?",
                "Discard changes?", MessageBoxButton.YesNo) == MessageBoxResult.Yes);
        }

        private void Confirm(object sender, RoutedEventArgs e)
        {
            HAS_NO_NAME.UpdateError();
            HAS_NO_YEAR.UpdateError();
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
            Form.Name = txName.Text;
            Form.YearGroup = (YearGroup)cmbxYear.SelectedItem;
            Form.Unfreeze();
            if (CommandType == CommandType.edit) {
                OriginalForm.UpdateWithClone(Form);
            } else
            {
                Form.Commit();
            }
            
            
        }
    }
}
