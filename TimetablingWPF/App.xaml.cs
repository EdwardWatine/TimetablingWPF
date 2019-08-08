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
            Properties["RecentFiles"] = new Queue<string>(6);
            Properties["Teachers"] = new List<Teacher>();
            Properties["Rooms"] = new List<Room>();
            Properties["Classes"] = new List<Class>();
            Properties["Subjects"] = new List<Subject>();
            Properties["Groups"] = new List<Subject>();
            Properties["Structure"] = new TimetableStructure(2, new List<TimetableStructurePeriod>()
            {
                new TimetableStructurePeriod("1", true),
                new TimetableStructurePeriod("2", true),
                new TimetableStructurePeriod("Brk", false),
                new TimetableStructurePeriod("3", true),
                new TimetableStructurePeriod("4", true),
                new TimetableStructurePeriod("Lch", false),
                new TimetableStructurePeriod("5", true)
            });

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
                    Utility.ShowErrorBox(e.Message);
                    throw e;
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
                    Utility.ShowErrorBox(e.Message);
                    throw e;
                }
            }
        }
    }

    internal class Utility
    {
        public static string Pluralize(int number, string word)
        {
            return number == 1 ? word : word.Pluralize();
        }
        public static Border setInternalBorder(FrameworkElement element)
        {
            return new Border()
            {
                Child = element,
                Style = (Style)Application.Current.Resources["GridLineInternal"]
            };
        }
        public static void ShowErrorBox(string msg)
        {
            MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

}
