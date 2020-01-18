using System;
using System.Collections.Generic;
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
using System.IO;
using Microsoft.Win32;
using static TimetablingWPF.FileHelpers;

namespace TimetablingWPF
{
    /// <summary>
    /// Interaction logic for FirstTime.xaml
    /// </summary>
    public partial class FirstTime : Page
    {
        public FirstTime()
        {

            Application.Current.MainWindow.SizeToContent = SizeToContent.WidthAndHeight;
            Application.Current.MainWindow.ResizeMode = ResizeMode.CanMinimize;

            InitializeComponent();

            IList<string> lines = Properties.Settings.Default.RECENT_FILES.Cast<string>().ToList();
            if (lines.Count == 0)
            {
                tbNoRecentFiles.Visibility = Visibility.Visible;
                icRecentFiles.Visibility = Visibility.Collapsed;
            }
            IEnumerable<Uri> uris = lines.Take(6).Select(x => new Uri(x));
            icRecentFiles.ItemsSource = uris;
        }

        private void Recent_File_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink link = (Hyperlink)e.Source;
            LoadData(link.Tag.ToString(), (worker_args) =>
            {
                if (worker_args.Cancelled)
                {
                    icRecentFiles.Items.Remove(link);
                }
                else
                {
                    RegisterOpenFile(link.Tag.ToString());
                    Window window = new MainWindow(true);
                    window.Show();
                    window.Activate();
                    Window.GetWindow(this).Close();
                }
            }, Window.GetWindow(this)
                );
        }

        public void OpenFile(object sender, RoutedEventArgs e)
        {
            string fpath = OpenFileDialogHelper();
            if (fpath != null)
            {
                LoadData(fpath, (worker_args) => {
                    if (!worker_args.Cancelled)
                    {
                        RegisterOpenFile(fpath);
                        new MainWindow(true).Show();
                        Window.GetWindow(this).Close();
                    }
                }, Window.GetWindow(this));
            }
        }

        private void LaunchManual(object sender, RoutedEventArgs e)
        {
            ImportDialog dialog = new ImportDialog(Window.GetWindow(this));
            dialog.ShowDialog();
        }
        private void NewTimetable(object sender, RoutedEventArgs e)
        {
            string fpath = SaveFileDialogHelper("Create New File");
            if (fpath != null)
            {
                TimetableStructureDialog structureDialog = new TimetableStructureDialog(Window.GetWindow(this), false);
                if (!structureDialog.ShowDialog() ?? false) { return; }
                SaveData(fpath);
                RegisterOpenFile(fpath);
                new MainWindow(true).Show();
                Window.GetWindow(this).Close();
            }
        }
    }

    public class ButtonProperties : DependencyObject
    {

        public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached("Text",
            typeof(string), typeof(ButtonProperties), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty ImageProperty = DependencyProperty.RegisterAttached("Image",
            typeof(ImageSource), typeof(ButtonProperties), new FrameworkPropertyMetadata(null));

        public static string GetText(DependencyObject d)
        {
            return (string)d.GetValue(TextProperty);
        }
        public static ImageSource GetImage(DependencyObject d)
        {
            return (ImageSource)d.GetValue(ImageProperty);
        }
        public static void SetText(DependencyObject d, string value)
        {
            d.SetValue(TextProperty, value);
        }
        public static void SetImage(DependencyObject d, ImageSource value)
        {
            d.SetValue(ImageProperty, value);
        }
    }

    
}

