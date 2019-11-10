using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using Humanizer;
using System.Runtime.Serialization;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Globalization;

namespace TimetablingWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Properties["APPLICATION_NAME"] = "Timetabler";
            Properties["USER_TYPES"] = new Type[] { typeof(Teacher), typeof(Subject), typeof(Lesson), typeof(Form), typeof(Group), typeof(Room) };
            Properties["GLOBAL_CULTURE"] = new CultureInfo("en-GB");
            Type ioc_type = typeof(InternalObservableCollection<>);
            foreach (Type type in (Type[])Properties["USER_TYPES"])
            {
                Properties[type] = Activator.CreateInstance(ioc_type.MakeGenericType(new Type[] { type }));
            }
            Properties[typeof(YearGroup)] = new List<YearGroup>() { new YearGroup("8") };
            Properties[typeof(TimetableStructure)] = new TimetableStructure(2, new List<TimetableStructurePeriod>()
            {
                new TimetableStructurePeriod("1", true),
                new TimetableStructurePeriod("2", true),
                new TimetableStructurePeriod("Brk", false),
                new TimetableStructurePeriod("3", true),
                new TimetableStructurePeriod("4", true),
                new TimetableStructurePeriod("Lch", false),
                new TimetableStructurePeriod("5", true)
            });
            InternalObservableCollection<Teacher> TestData = new InternalObservableCollection<Teacher>
            {
                new Teacher(){ Name="Mr Worth" },
                new Teacher(){ Name="Mr Henley" }
            };
            foreach (Teacher teacher in TestData)
            {
                teacher.Commit();
            }
            InternalObservableCollection<Subject> TestSubjects = new InternalObservableCollection<Subject>() {
                new Subject(){ Name = "Science" },
                new Subject() { Name = "Timetabling" }
            };
            foreach (Subject subject in TestSubjects) { subject.Commit(); }
            MainWindow window = new MainWindow();
            foreach (Type type in (Type[])Properties["USER_TYPES"])
            {
                window.GetMainPage().NewDataSetTab(type);
            }
            FileHandlers.SaveData(@"C:\Users\02ewa\Desktop\testdata.ttbl");
            window.Show();
        }

    }
}
