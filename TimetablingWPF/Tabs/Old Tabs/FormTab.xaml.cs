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
    public partial class FormTab : TabBase
    {
        public FormTab(Form form, CommandType commandType)
        {
            InitializeComponent();

            ErrManager = new ErrorManager(spErrors);
            CommandType = commandType;
            OriginalForm = form;
            Form = commandType == CommandType.@new ? form : (Form)form.Clone();
            Form.Freeze();
            tbTitle.Text = "Create a new Form";
            txName.Text = form.Name;
            txName.SelectionStart = txName.Text.Length;
            cmbxLesson.ItemsSource = GetDataContainer().Lessons;
            cmbxYear.ItemsSource = GetDataContainer().YearGroups;
            ilLessons.ItemsSource = Form.Lessons;
            if (commandType != CommandType.@new) ilLessons.ListenToCollection(OriginalForm.Lessons);
            HAS_NO_NAME = GenericHelpers.GenerateNameError(ErrManager, txName, "Form");
            HAS_NO_YEAR = new ErrorContainer((e) => cmbxYear.SelectedItem == null, (e) => "No year group has been selected.", ErrorType.Warning);
            cmbxYear.comboBox.SelectionChanged += delegate (object o, SelectionChangedEventArgs e) { HAS_NO_YEAR.UpdateError(); };
            //Errors
        }

        private void LessonButtonClick(object sender, RoutedEventArgs e)
        {

            Lesson lesson = (Lesson)cmbxLesson.SelectedItem;
            if (lesson != null && !Form.Lessons.Contains(lesson))
            {
                Form.Lessons.Add(lesson);
            }
        }

        private readonly Form Form;
        private readonly Form OriginalForm;
        private readonly ErrorContainer HAS_NO_YEAR;
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
            Form.Name = txName.Text.Trim();
            Form.YearGroup = (Year)cmbxYear.SelectedItem;
            Form.Unfreeze();
            if (CommandType == CommandType.edit) {
                OriginalForm.UpdateWithClone(Form);
            } else
            {
                Form.Commit();
            }
            MainPage.CloseTab(this);
            
        }
    }
}
