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
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;

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
            Properties[Teacher.ListName] = new InternalObservableCollection<Teacher>();
            Properties[Room.ListName] = new InternalObservableCollection<Room>();
            Properties[Band.ListName] = new InternalObservableCollection<Band>();
            Properties[Subject.ListName] = new InternalObservableCollection<Subject>();
            Properties[Group.ListName] = new InternalObservableCollection<Group>();
            Properties[TimetableStructure.ListName] = new TimetableStructure(2, new List<TimetableStructurePeriod>()
            {
                new TimetableStructurePeriod("1", true),
                new TimetableStructurePeriod("2", true),
                new TimetableStructurePeriod("Brk", false),
                new TimetableStructurePeriod("3", true),
                new TimetableStructurePeriod("4", true),
                new TimetableStructurePeriod("Lch", false),
                new TimetableStructurePeriod("5", true)
            });
            ObservableCollection<Teacher> TestData = new ObservableCollection<Teacher>
            {
                new Teacher(){Name="Mr Worth" },
                new Teacher(){Name="Mr Henley" }
            };
            foreach (Teacher teacher in TestData)
            {
                teacher.Commit();
            }
            ObservableCollection<Subject> TestSubjects = new ObservableCollection<Subject>() {
                new Subject(){Name = "Science" },
                new Subject() { Name = "Timetabling" }
            };
            foreach (Subject subject in TestSubjects) { subject.Commit(); }
            MainWindow window = new MainWindow();
            foreach (Type type in new Type[] { typeof(Teacher), typeof(Subject), typeof(Band) })
            {
                window.GetMainPage().NewDataClassTab(type);
            }
            window.Show();
        }

    }

    internal class FileFunctions
    {
        public static void LoadFile(string fpath)
        {
            Uri URI = new Uri(fpath);
            using (FileStream fs = new FileStream(URI.LocalPath, FileMode.Open))
            {
                
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    IDictionary<object, object> dict = (Dictionary<object, object>)formatter.Deserialize(fs);
                    Application.Current.Properties.Clear();
                    foreach (KeyValuePair<object, object> entry in dict)
                    {
                        Application.Current.Properties[entry.Key] = entry.Value;
                    }
                }
                catch (Exception e)
                {
                    VisualHelpers.ShowErrorBox(e.Message);
                }
            }
        }
        public static void SaveFile(string fpath)
        {
            Uri URI = new Uri(fpath);
            using (FileStream fs = new FileStream(URI.LocalPath, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    formatter.Serialize(fs, Application.Current.Properties);
                }
                catch (Exception e)
                {
                    VisualHelpers.ShowErrorBox(e.Message);
                }
            }
        }
    }
}
