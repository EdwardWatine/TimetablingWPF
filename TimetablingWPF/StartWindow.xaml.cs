﻿using Humanizer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
using static TimetablingWPF.FileHelpers;

namespace TimetablingWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class StartWindow : Window
    {
        public StartWindow()
        {
            InitializeComponent();
            IEnumerable<string> lines = Properties.Settings.Default.RECENT_FILES.Cast<string>(); // access the recent files
            if (!lines.Any())
            {
                tbNoRecentFiles.Visibility = Visibility.Visible; // show a message to the user
                icRecentFiles.Visibility = Visibility.Collapsed;
            }
            uris = new ObservableCollection<Uri>(lines.Take(6).Select(x => new Uri(x)));
            icRecentFiles.ItemsSource = uris;
            Show();
            string fpath = Environment.GetCommandLineArgs().ElementAtOrDefault(1); // enable double click for files
            if (!string.IsNullOrEmpty(fpath))
            {
                OpenFileFromPath(fpath);
            }
        }
        private readonly ObservableCollection<Uri> uris;
        private void Recent_File_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink link = (Hyperlink)e.Source;
            Uri tag = (Uri)link.Tag;
            LoadData(tag.LocalPath, (worker_args) =>
            {
                if (worker_args.Result is FileNotFoundException)
                {
                    uris.Remove(tag); // remove the link if the file is not found
                    return;
                }
                new MainWindow(true).Show();
                Close();
            }, owner: this); 
        }

        private void OpenFile(object sender, RoutedEventArgs e)
        {
            string fpath = OpenFileDialogHelper();
            if (fpath != null)
            {
                OpenFileFromPath(fpath);
            }
        }

        private void OpenFileFromPath(string fpath)
        {
            LoadData(fpath, (worker_args) => {
                new MainWindow(true).Show();
                Close();
            }, owner: this);
        }

        private void LaunchManual(object sender, RoutedEventArgs e)
        {
            
        }
        private void NewTimetable(object sender, RoutedEventArgs e)
        {
            string fpath = SaveFileDialogHelper("Create New File");
            if (fpath != null)
            {
                TimetableStructureDialog structureDialog = new TimetableStructureDialog(this, false);
                if (!structureDialog.ShowDialog() ?? false) { return; }
                SaveData(fpath);
                RegisterOpenFile(fpath);
                new MainWindow(true).Show();
                Close();
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
