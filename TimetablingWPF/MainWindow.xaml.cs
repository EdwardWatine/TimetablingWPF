using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using static TimetablingWPF.FileFunctions;

namespace TimetablingWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            RelationalList<Subject> TestSubjects = new RelationalList<Subject>("Teachers") {

                new Subject("Science"),
                new Subject("Timetabling")
            };
            foreach (Subject subject in TestSubjects) { subject.Commit(); }
            ObservableCollection<Teacher> TestData = new ObservableCollection<Teacher>
            {
                new Teacher(){Name="Mr Worth" },
                new Teacher(){Name="Mr Henley" }
            };

            //Subject Science = new Subject("Science");

            //Room sroom = new Room("Science Rooms", 5);
            Content = new MainPage(TestData);
            return;
            if (AppDomain.CurrentDomain.ActivationContext == null)
            {
                Content = new FirstTime();
                return;
            }
            var file = AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData;
            if (file != null && file.Length > 0)
            {
                LoadFile(file[0]);
                return;
            }
            Content = new FirstTime();
            return;

        }
    }
}
