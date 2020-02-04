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
using TimetablingWPF.StructureClasses;

namespace TimetablingWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Properties["APPLICATION_NAME"] = "Timetabler";
            Properties["CURRENT_FILE_PATH"] = null;
            Properties["USER_TYPES"] = new Type[] { typeof(Teacher), typeof(Subject), typeof(Lesson), typeof(Form), typeof(Group), typeof(Room) };
            DataContainer data = new DataContainer();
            Properties["CURRENT_DATA"] = data;
            data.YearGroups.Add(new YearGroup("8"));
            TimetableStructure.SetData(new List<TimetableStructureWeek>()
            {
                new TimetableStructureWeek("A", DataHelpers.ShortenedDaysOfTheWeek,
                new List<string>(){"1", "2", "Brk", "3", "4", "Lch", "5" }, new List<int>(){2, 5, 9, 12, 16, 19, 23, 26, 30, 33 }),
                new TimetableStructureWeek("B", DataHelpers.ShortenedDaysOfTheWeek,
                new List<string>(){"1", "2", "Brk", "3", "4", "Lch", "5" }, new List<int>(){2, 5, 9, 12, 16, 19, 23, 26, 30, 33 })
            });
            SessionEnding += delegate (object sender, SessionEndingCancelEventArgs e)
            {
                if (MainWindow is StartWindow) { return; }
                MessageBoxResult result = VisualHelpers.ShowUnsavedBox();
                if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                if (result == MessageBoxResult.Yes)
                {
                    FileHelpers.SaveData(FileHelpers.GetCurrentFilePath());
                }
            };
            
        }

    }
}
