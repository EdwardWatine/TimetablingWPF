using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.VisualStyles;
using static TimetablingWPF.DataHelpers;
using ObservableComputations;

namespace TimetablingWPF
{
    public class RecentFileManager : INotifyPropertyChanged
    {
        public RecentFileManager()
        {
            UpdateRecentFilePaths();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollectionExtended<string> FilePaths { get; private set; }
        public void UpdateRecentFilePaths()
        {
            FilePaths = Properties.Settings.Default.RECENT_FILES.Cast<string>().Take(LocalSettings.RecentListSize.Value + 1).ToObservable();
            FilePaths.Remove(App.FilePath);
            if (FilePaths.Count > LocalSettings.RecentListSize.Value)
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
        public static void LoadData(string fpath, Action<RunWorkerCompletedEventArgs> done = null, Action<RunWorkerCompletedEventArgs> onCancel = null, 
            Action<RunWorkerCompletedEventArgs> exception = null, Window owner = null, bool save = true)
        {
            BackgroundWorker worker = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            worker.Dispose(); //Doesn't do anything
            Application.Current.Dispatcher.Invoke(() =>
            {
                string newPath = null;
                LoadingDialog box = new LoadingDialog("Loading...")
                {
                    Owner = owner
                };
                SavingMode? mode = null;
                long skip = 0;
                MemoryStream mstream = null;
                BinaryReader reader = null;
                worker.DoWork += delegate (object sender, DoWorkEventArgs e)
                {
                    if (!File.Exists(fpath))
                    {
                        VisualHelpers.ShowErrorBox($"File path {fpath} does not exist.", "File not found!");
                        RecentFilesRemove(fpath);
                        e.Result = new FileNotFoundException();
                        return;
                    }
                    mstream = new MemoryStream(File.ReadAllBytes(fpath));
                    reader = new BinaryReader(mstream);
                    mode = (SavingMode)reader.ReadInt32();
                    skip = mstream.Position;
                    if (mode == SavingMode.Backup)
                    {
                        if (!save)
                        {
                            VisualHelpers.ShowErrorBox("Backup files cannot be used for this function. Please restore it first.");
                            e.Cancel = true;
                            return;
                        }
                        newPath = SaveFileDialogHelper("Restore Backup");
                        if (newPath == null)
                        {
                            e.Cancel = true;
                            return;
                        }
                        _ = reader.ReadString();
                        skip = mstream.Position;
                    }
                    e.Result = Loading.StartLoad(reader, Version.Parse(reader.ReadString()), worker, e);
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
                    mstream?.Dispose();
                    reader?.Dispose();
                    if (e.Cancelled)
                    {
                        onCancel?.Invoke(e);
                        return;
                    }
                    if (e.Result is Exception)
                    {
                        exception?.Invoke(e);
                        return;
                    }
                    if (e.Error != null)
                    {
                        VisualHelpers.ShowErrorBox($"A {e.Error.GetType().Name} error occured:  {e.Error.Message}");
                        return;
                    }
                    if (save)
                    {
                        DataContainer data = (DataContainer)e.Result;
                        SetDataContainer(data);
                        App.Data.UpdateSave();
                        RegisterOpenFile(mode == SavingMode.Normal ? fpath : newPath);
                        using (FileStream stream = new FileStream(fpath, FileMode.Open))
                        {
                            stream.Seek(skip, SeekOrigin.Begin);
                            if (mode == SavingMode.Backup)
                            {
                                RestoreBackup(stream, newPath);
                            }
                        }
                        if (mode == SavingMode.Backup)
                        {
                            File.Delete(fpath);
                        }
                    }
                    done?.Invoke(e);
                };
                box.Show();
                worker.RunWorkerAsync();
            });
        }
        public static void RestoreBackup(FileStream stream, string newPath)
        {
            using (FileStream nfile = File.OpenWrite(newPath))
            {
                nfile.Write(BitConverter.GetBytes((int)SavingMode.Normal), 0, sizeof(int));
                stream.CopyTo(nfile);
            }
        }
        public static void SaveData(string fpath)
        {
            SaveDataToFile(fpath);
            App.Data.UpdateSave();
        }
        public static readonly BackgroundTask SaveTask = new BackgroundTask("Saving data", "Saving the data of the current application.");
        private static void SaveDataToFile(string fpath)
        {
            BackgroundTaskManager.Tasks.Add(SaveTask);
            new Thread(new ThreadStart(() =>
            {
                using (MemoryStream all_data = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(all_data))
                    {
                        DataContainer data = (DataContainer)App.Data.ToContainer();
                        writer.Write((int)SavingMode.Normal);
                        Saving.StartSave(writer, data);
                        all_data.Seek(0, SeekOrigin.Begin);
                        using (FileStream stream = File.OpenWrite(fpath))
                        {
                            all_data.CopyTo(stream);
                            BackgroundTaskManager.Tasks.Remove(SaveTask);
                        }
                    }
                }
            })).Start();
        }
        public static string OpenFileDialogHelper(string title = "Open")
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = $"Timetable files (*{App.Ext})|*{App.Ext}|Backup files (*{App.BkExt})|*{App.BkExt}|All files (*.*)|*.*",
                InitialDirectory = Properties.Settings.Default.LAST_ACCESSED_PATH,
                ValidateNames = true,
                DefaultExt = App.Ext,
                Title = title
            };
            dialog.CustomPlaces.Add(new FileDialogCustomPlace(App.BkPath));
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
                Filter = $"Timetable files (*{App.Ext})|*{App.Ext}|Backup files (*{App.BkExt})|*{App.BkExt}|All files (*.*)|*.*",
                InitialDirectory = Properties.Settings.Default.LAST_ACCESSED_PATH,
                ValidateNames = true,
                OverwritePrompt = true,
                DefaultExt = App.Ext,
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
            App.FilePath = fpath;
            SetWindowHeaders();
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
            string header = $"{App.Name} - {App.FilePath}";
            if (App.Data.Unsaved)
            {
                header = "*" + header + " [unsaved]";
            }
            return header;
        }
        public static string MakeBackupPath(string fpath)
        {
            return Path.Combine(App.BkPath, fpath.GetHashCode().ToString(CultureInfo.InvariantCulture)) + App.BkExt;
        }
        public static IEnumerable<string> GetBackups(string pathMatch = null)
        {
            string hashpath = "";
            if (!string.IsNullOrEmpty(pathMatch))
            {
                hashpath = MakeBackupPath(pathMatch);
                if (File.Exists(hashpath))
                {
                    if (ReadBackupPath(hashpath) == pathMatch)
                    {
                        yield return hashpath;
                        yield break;
                    }
                }
            }
            IEnumerable<string> paths = Directory.GetFiles(App.BkPath, "*" + App.BkExt);
            if (string.IsNullOrEmpty(pathMatch))
            {
                foreach (string path in paths)
                {
                    yield return path;
                }
                yield break;
            }
            foreach (string path in paths)
            {
                if (path == hashpath)
                {
                    continue;
                }
                if (ReadBackupPath(path) == pathMatch)
                {
                    yield return path;
                }
            }
            yield break;
        }
        public static string ReadBackupPath(string fpath)
        {
            try
            {
                using (FileStream stream = new FileStream(fpath, FileMode.Open))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        _ = reader.ReadInt32();
                        string s = reader.ReadString();
                        return s;
                    }
                }
            }
            catch (IOException)
            {
                return string.Empty;
            }
        }
        public static void WriteBackup(string fpath)
        {
            using (MemoryStream all_data = new MemoryStream()) {
                using (BinaryWriter writer = new BinaryWriter(all_data))
                {
                    DataContainer data = (DataContainer)App.Data.ToContainer();
                    writer.Write((int)SavingMode.Backup);
                    writer.Write(App.FilePath);
                    Saving.StartSave(writer, data);
                    all_data.Seek(0, SeekOrigin.Begin);
                    using (FileStream stream = File.OpenWrite(fpath))
                    {
                        all_data.CopyTo(stream);
                    }
                }
            }
        }
    }

    public enum SavingMode
    {
        Normal,
        Backup
    }

}
