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
            Hyperlink link = e.Source as Hyperlink;
            if (LoadData(link.Tag.ToString(), () => { new MainWindow(true).Show(); Window.GetWindow(this).Close(); }, Window.GetWindow(this)))
            {
                RegisterOpenFile(link.Tag.ToString());
            }
        }

        public void OpenFile(object sender, RoutedEventArgs e)
        {
            string fpath = OpenFileDialogHelper();
            if (fpath != null)
            {
                if (LoadData(fpath, () => { new MainWindow(true).Show(); Window.GetWindow(this).Close(); }, Window.GetWindow(this)))
                {
                    RegisterOpenFile(fpath);
                }
            }
        }

        private void NewByImport(object sender, RoutedEventArgs e)
        {
            string fpath = OpenFileDialogHelper();
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
            return (string)parameter == "filename" ? System.IO.Path.GetFileName(URI.AbsolutePath) : URI.LocalPath;
        }
        public object ConvertBack(object value, Type targetType, object paramter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}

