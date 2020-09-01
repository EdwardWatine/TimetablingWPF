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
using System.Reflection;

namespace TimetablingWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 
    public partial class App : Application
    {
        public App()
        {
            //GenericHelpers.LogTime("A");
            InitializeComponent();
            Data.SetTimer();
            Directory.CreateDirectory(BkPath);
            EventManager.RegisterClassHandler(typeof(FrameworkElement), UIElement.GotFocusEvent, new RoutedEventHandler((s, e) => ((FrameworkElement)s).FocusVisualStyle = null), true);
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
                    FileHelpers.SaveData(FilePath);
                }
            };
            
            //GenericHelpers.LogTime("B");
        }
        public const string Name = "Timetabler";
        public static string FilePath { get; set; }
        public static SingletonDataContainer Data { get; } = SingletonDataContainer.Instance;
        public static readonly string DocPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Name);
        public static readonly string BkPath = Path.Combine(DocPath, "Backup");
        public const string Ext = ".ttbl";
        public const string BkExt = ".ttbk";

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer element = (ScrollViewer)sender;
            if (element.Tag == null)
            {
                element.Tag = element.FindChild("PART_bdMain");
            }
            if (e.VerticalChange == 0)
            {
                return;
            }
            VisualStateManager.GoToElementState((FrameworkElement)element.Tag, "ManualNormal", true);
            VisualStateManager.GoToElementState((FrameworkElement)element.Tag, "ManualScroll", true);
        }
    }
}
