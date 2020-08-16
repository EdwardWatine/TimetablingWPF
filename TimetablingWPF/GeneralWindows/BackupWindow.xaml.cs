using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static TimetablingWPF.FileHelpers;
using ObservableComputations;

namespace TimetablingWPF
{
    /// <summary>
    /// Interaction logic for FullBackupWindow.xaml
    /// </summary>
    public partial class BackupWindow : Window
    {
        public IList<BackupInfo> Paths { get; }
        public Action<string> LoadingAction { get; }
        public BackupWindow(Window window, Action<string> loadingAction, bool filter = false)
        {
            InitializeComponent();
            Owner = window;
            LoadingAction = loadingAction;
            IEnumerable<BackupInfo> temp = GetBackups().Select(s => new BackupInfo(s)).Where(s => !filter || s.FileExists);
            if (filter)
            {
                tbMain.Text = "The following files were not saved the last time that they were closed:";
            }
            else
            {
                temp = temp.OrderBy(b => b.FileExists ? 1 : 0);
            }
            Paths = temp.ToObservable();
            if (!temp.Any())
            {
                lbMain.Items.Add("There are no backup files to display...");
                return;
            }
            lbMain.ItemsSource = Paths;
        }
        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            BackupInfo pinfo = (BackupInfo)lbMain.SelectedItem;
            File.Delete(pinfo.BackupPath);
            Paths.RemoveAt(lbMain.SelectedIndex);
            if (Paths.Count == 0)
            {
                lbMain.ItemsSource = null;
                lbMain.Items.Add("There are no backup files to display...");
            }
        }
        private void RestoreClick(object sender, RoutedEventArgs e)
        {
            string path = SaveFileDialogHelper("Restore Backup");
            BackupInfo pinfo = (BackupInfo)lbMain.SelectedItem;
            if (path == null)
            {
                return;
            }
            using (FileStream stream = new FileStream(pinfo.BackupPath, FileMode.Open))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    _ = reader.ReadInt32(); // dispose of backup flag
                    _ = reader.ReadString(); // dispose of the filename
                    RestoreBackup(stream, path);
                }
            }
            File.Delete(pinfo.BackupPath);
            DeleteClick(null, null);
        }

        private void RestoreLoad(object sender, RoutedEventArgs e)
        {
            BackupInfo pinfo = (BackupInfo)lbMain.SelectedItem;
            if (!string.IsNullOrEmpty(App.FilePath))
            {
                if (App.Data.Unsaved)
                {
                    bool? result = VisualHelpers.UnsavedDialog();
                    if (result == null)
                    {
                        return;
                    }
                    if (result == true)
                    {
                        SaveData(App.FilePath);
                    }
                }
            }
            Close();
            LoadingAction(pinfo.BackupPath);
        }
    }

    public struct BackupInfo
    {
        public BackupInfo(string path)
        {
            Path = ReadBackupPath(path);
            FileExists = File.Exists(Path);
            Filename = System.IO.Path.GetFileName(Path);
            BackupPath = path;
        }
        public bool FileExists { get; }
        public string Path { get; }
        public string Filename { get; }
        public string BackupPath { get; }
    }
}
