using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static TimetablingWPF.DataHelpers;

namespace TimetablingWPF
{
    public class RecentFileManager : INotifyPropertyChanged
    {
        public RecentFileManager()
        {
            UpdateRecentFilePaths();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public IList<string> FilePaths { get; private set; }
        public void UpdateRecentFilePaths()
        {
            FilePaths = Properties.Settings.Default.RECENT_FILES.Cast<string>().Take(TimetableSettings.RecentListSize + 1).ToList();
            FilePaths.Remove(FileHelpers.GetCurrentFilePath());
            if (FilePaths.Count > TimetableSettings.RecentListSize)
            {
                FilePaths.RemoveAt(FilePaths.Count - 1);
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilePaths)));
        }
    }
    public static class FileHelpers
    {
        public static RecentFileManager RecentFileManager { get; } = new RecentFileManager();
        public static void RegisterOpenFile(string fpath)
        {
            SetCurrentFilePath(fpath);
            Properties.Settings.Default.RECENT_FILES.Remove(fpath);
            Properties.Settings.Default.RECENT_FILES.Insert(0, fpath);
            Properties.Settings.Default.Save();
            RecentFileManager.UpdateRecentFilePaths();
        }
        public static void RecentFilesRemove(string fpath)
        {
            Properties.Settings.Default.RECENT_FILES.Remove(fpath);
            Properties.Settings.Default.Save();
            RecentFileManager.UpdateRecentFilePaths();
        }
        public static void LoadData(string fpath, Action<RunWorkerCompletedEventArgs> done = null, Action<RunWorkerCompletedEventArgs> onCancel = null, Window owner = null, bool save = true)
        {
            BackgroundWorker worker = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            worker.Dispose(); //Doesn't do anything
            Application.Current.Dispatcher.Invoke(() =>
            {
                LoadingDialog box = new LoadingDialog("Loading...")
                {
                    Owner = owner
                };
                worker.DoWork += delegate (object sender, DoWorkEventArgs e)
                {
                    if (!File.Exists(fpath))
                    {
                        VisualHelpers.ShowErrorBox($"File path {fpath} does not exist.", "File not found!");
                        RecentFilesRemove(fpath);
                        e.Result = new FileNotFoundException();
                        return;
                    }
                    FileStream fstream = new FileStream(fpath, FileMode.Open);
                    BinaryReader reader = new BinaryReader(fstream);
                    e.Result = Loading.StartLoad(reader, Version.Parse(reader.ReadString()), worker, e);
                    reader.Dispose();
                    fstream.Close();
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                    }
                };
                worker.ProgressChanged += delegate (object sender, ProgressChangedEventArgs e)
                {
                    box.pbProgressBar.Value = e.ProgressPercentage;
                    box.tbTextBlock.Text = (string)e.UserState;
                };
                worker.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs e)
                {
                    box.Close();
                    if (e.Error != null)
                    {
                        VisualHelpers.ShowErrorBox($"A {e.Error.GetType().Name} error occured:  {e.Error.Message}");
                        return;
                    }
                    if (e.Cancelled)
                    {
                        onCancel?.Invoke(e);
                        return;
                    }
                    if (save && !(e.Result is Exception))
                    {
                        DataContainer data = (DataContainer)e.Result;
                        SetDataContainer(data);
                        GetDataContainer().UpdateSave();
                    }
                    done?.Invoke(e);
                };
                box.Show();
                worker.RunWorkerAsync();
            });
        }
        public static void SaveData(string fpath)
        {
            SaveDataToFile(fpath);
            GetDataContainer().UpdateSave();
        }
        private static void SaveDataToFile(string fpath)
        {
            MemoryStream all_data = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(all_data);
            DataContainer data = (DataContainer)GetDataContainer().Clone();
            Saving.StartSave(writer, data);
            all_data.Seek(0, SeekOrigin.Begin);
            FileStream stream = File.OpenWrite(fpath);
            all_data.CopyTo(stream);
            stream.Close();
            writer.Close();
            all_data.Close();
        }
        public static string OpenFileDialogHelper(string title = "Open")
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Timetable files (*.TTBL)|*.TTBL|All files (*.*)|*.*",
                InitialDirectory = Properties.Settings.Default.LAST_ACCESSED_PATH,
                ValidateNames = true,
                DefaultExt = ".ttbl",
                Title = title
            };
            if (dialog.ShowDialog() ?? false)
            {
                Properties.Settings.Default.LAST_ACCESSED_PATH = dialog.FileName;
                Properties.Settings.Default.Save();
                return dialog.FileName;
            }
            return null;
        }
        public static string SaveFileDialogHelper(string title = "Save As")
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "Timetable files (*.TTBL)|*.TTBL|All files (*.*)|*.*",
                InitialDirectory = Properties.Settings.Default.LAST_ACCESSED_PATH,
                ValidateNames = true,
                OverwritePrompt = true,
                DefaultExt = ".ttbl",
                AddExtension = true,
                Title = title
            };
            if (dialog.ShowDialog() ?? false)
            {
                Properties.Settings.Default.LAST_ACCESSED_PATH = dialog.FileName;
                Properties.Settings.Default.Save();
                return dialog.FileName;
            }
            return null;
        }
        public static void SetCurrentFilePath(string fpath)
        {
            Application.Current.Properties["CURRENT_FILE_PATH"] = fpath;
            SetWindowHeaders();
        }
        public static string GetCurrentFilePath()
        {
            return (string)Application.Current.Properties["CURRENT_FILE_PATH"];
        }
        public static void SetWindowHeaders()
        {
            string header = GetWindowHeader();
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is WindowBase)
                    {
                        window.Title = header;
                    }
                }
            });
        }
        public static string GetWindowHeader()
        {
            string header = $"Timetabler - {Application.Current.Properties["CURRENT_FILE_PATH"]}";
            if (GetDataContainer().Unsaved)
            {
                header = "*" + header + " [unsaved]";
            }
            return header;
        }
    }

}
