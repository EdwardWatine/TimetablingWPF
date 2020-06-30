using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TimetablingWPF
{
    /// <summary>
    /// Interaction logic for CheckboxDialog.xaml
    /// </summary>
    public partial class ImportDialog : Window
    {
        public ImportDialog(Window owner)
        {
            Owner = owner;
            InitializeComponent();
        }
        private static ImportOption CheckableToImport(ToggleButton importtoggle, ToggleButton replacetoggle = null, ToggleButton mergetoggle = null)
        {
            if (importtoggle?.IsChecked ?? false) return ImportOption.Import;
            if (replacetoggle?.IsChecked ?? false) return ImportOption.Replace;
            if (mergetoggle?.IsChecked ?? false) return ImportOption.Merge;
            return ImportOption.NoImport;
        }
        public ImportOption ImportTimetable => CheckableToImport(TimetableStructureImport);
        public ImportOption ImportTeacher => CheckableToImport(TeacherImport, TeacherReplace, TeacherMerge);
        public ImportOption ImportLesson => CheckableToImport(LessonImport, LessonReplace, LessonMerge);
        public ImportOption ImportSubject => CheckableToImport(SubjectImport, SubjectReplace, SubjectMerge);
        public ImportOption ImportForm => CheckableToImport(FormImport, FormImportFalse, FormMerge);
        public ImportOption ImportGroup => CheckableToImport(GroupImport, GroupReplace, GroupMerge);
        public ImportOption ImportAssignment => CheckableToImport(AssignmentImport);
        private void Confirm(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

    }
    public enum ImportOption
    {
        NoImport,
        Import,
        Replace,
        Merge
    }
}
