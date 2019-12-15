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
            InitializeComponent();
            Properties["APPLICATION_NAME"] = "Timetabler";
            Properties["CURRENT_FILE_PATH"] = null;
            Properties["USER_TYPES"] = new Type[] { typeof(Teacher), typeof(Subject), typeof(Lesson), typeof(Form), typeof(Group), typeof(Room) };
            Type ioc_type = typeof(InternalObservableCollection<>);
            foreach (Type type in (Type[])Properties["USER_TYPES"])
            {
                Properties[type] = Activator.CreateInstance(ioc_type.MakeGenericType(new Type[] { type }));
            }
            Properties[typeof(YearGroup)] = new List<YearGroup>() { new YearGroup("8") };
            TimetableStructure.SetData(2, new List<TimetableStructurePeriod>()
            {
                new TimetableStructurePeriod("1", true),
                new TimetableStructurePeriod("2", true),
                new TimetableStructurePeriod("Brk", false),
                new TimetableStructurePeriod("3", true),
                new TimetableStructurePeriod("4", true),
                new TimetableStructurePeriod("Lch", false),
                new TimetableStructurePeriod("5", true)
            });
            StartWindow window = new StartWindow();
            window.Show();
        }

    }
}
