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
using static TimetablingWPF.FileFunctions;

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

            IList<string> lines = (IList<string>)Properties.Settings.Default.RECENT_FILES;
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
            Hyperlink link = e.Source as Hyperlink;
            LoadFile(link.Tag.ToString());
        }
        
        public void OpenFile(object sender, RoutedEventArgs e)
        {
            string fpath = FileDialog();
            if (fpath!=null)
            {
                LoadFile(fpath);
            }
        }

        public string FileDialog()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Timetable files (*.TTBL)|*.TTBL|All files (*.*)|*.*",
                InitialDirectory = TimetablingWPF.Properties.Settings.Default.LAST_ACCESSED_PATH,
                ValidateNames = true
            };
            if (dialog.ShowDialog() == true)
            {
                TimetablingWPF.Properties.Settings.Default.LAST_ACCESSED_PATH = dialog.FileName;
                return dialog.FileName;
            }
            return null;
        }

        private void NewByImport(object sender, RoutedEventArgs e)
        {
            string fpath = FileDialog();
            if (fpath == null) { return; }
            CheckboxDialog cbDialog = new CheckboxDialog(Application.Current.MainWindow, new string[] {
                "Subjects", "Teachers", "Rooms", "Forms", "\n", "Sets", "Timetables"});
            if (cbDialog.ShowDialog() != true)
            {
                return;
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
            throw new NotImplementedException("GetterText");
        }
        public static ImageSource GetImage(DependencyObject d)
        {
            throw new NotImplementedException("GetterImage");
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

    public class URISetatter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Uri URI = (Uri)value;
            return (string)parameter == "filename" ? System.IO.Path.GetFileName(URI.AbsolutePath) : URI.AbsolutePath;
        }
        public object ConvertBack(object value, Type targetType, object paramter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}

