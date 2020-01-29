using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static TimetablingWPF.DataHelpers;

namespace TimetablingWPF
{
    public static class FileHelpers
    {
        public static void WriteIntEnum(IEnumerable<int> enumerable, BinaryWriter writer)
        {
            WriteList(enumerable.ToList(), (i, index) => writer.Write(i), writer);
        }
        public static void WriteBDCEnum<T>(IEnumerable<T> enumerable, BinaryWriter writer, Action<T> action) where T : BaseDataClass
        {
            IList<T> list = enumerable.ToList();
            writer.Write(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                T obj = list[i];
                obj.StorageIndex = i;
                writer.Write(obj.Name);
                action(obj);
            }
        }
        public static void WriteList<T>(IList<T> list, Action<T, int> action, BinaryWriter writer)
        {
            writer.Write(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                action(list[i], i);
            }
        }
        public static void LoadEnum(Action action, BinaryReader reader)
        {
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                action();
            }
        }
        public static IList<T> LoadAndReturnList<T>(Func<T> func, BinaryReader reader)
        {
            int count = reader.ReadInt32();
            IList<T> retval = new List<T>(count);
            for (int i = 0; i < count; i++)
            {
                retval.Add(func());
            }
            return retval;
        }
        public static IList<int> LoadAndReturnIntList(BinaryReader reader)
        {
            return LoadAndReturnList(() => reader.ReadInt32(), reader);
        }
        public static void LoadBDCEnum<T>(Action<T> action, BinaryReader reader) where T : BaseDataClass, new()
        {
            LoadEnum(() =>
            {
                T obj = new T() { Name = reader.ReadString() };
                action(obj);
                obj.Commit();
            }, reader);
        }
        public static void RegisterOpenFile(string fpath)
        {
            SetCurrentFilePath(fpath);
            Properties.Settings.Default.RECENT_FILES.Remove(fpath);
            Properties.Settings.Default.RECENT_FILES.Insert(0, fpath);
            Properties.Settings.Default.Save();
        }
        public static void RecentFilesRemove(string fpath)
        {
            Properties.Settings.Default.RECENT_FILES.Remove(fpath);
            Properties.Settings.Default.Save();
        }
        public static void LoadData(string fpath, Action<RunWorkerCompletedEventArgs> done = null, Window owner = null)
        {
            BackgroundWorker worker = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            worker.Dispose(); //Doesn't do anything
            if (!File.Exists(fpath))
            {
                VisualHelpers.ShowErrorBox($"File path {fpath} does not exist.", "File not found!");
                RecentFilesRemove(fpath);
                worker.CancelAsync();
                return;
            }
            Application.Current.Dispatcher.Invoke(() =>
            {
                LoadingDialog box = new LoadingDialog("Loading...")
                {
                    Owner = owner
                };
                worker.DoWork += delegate (object sender, DoWorkEventArgs e)
                {
                    FileStream fstream = new FileStream(fpath, FileMode.Open);
                    BinaryReader reader = new BinaryReader(fstream);
                    LoadingFormats.GetLoadingDelegate(reader.ReadInt32()).Invoke(fpath, reader, worker, e);
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
                    done?.Invoke(e);
                };
                box.Show();
                worker.RunWorkerAsync();
            });
        }
        public static void SaveData(string fpath)
        {
            SaveDataToFile(fpath);
        }
        public static void SaveDataToFile(string fpath)
        {
            MemoryStream all_data = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(all_data);
            IList<YearGroup> year_list = GetData<YearGroup>();
            writer.Write(LoadingFormats.LATEST);
            WriteList(TimetableStructure.Weeks, (w, i) =>
            {
                writer.Write(w.Name);
                WriteList(w.DayNames, (d, i2) =>
                {
                    writer.Write(d);
                }, writer);
                WriteList(w.PeriodNames, (p, i2) =>
                {
                    writer.Write(p);
                }, writer);
                WriteIntEnum(w.UnavailablePeriods, writer);
            }, writer);
            WriteList(year_list, (y, i) =>
            {
                y.StorageIndex = i;
                writer.Write(y.Year);
            }, writer);
            WriteBDCEnum(GetData<Teacher>(), writer, t =>
            {
                writer.Write(t.MaxPeriodsPerCycle);
                writer.Write(t.UnavailablePeriods.Count);
                WriteIntEnum(t.UnavailablePeriods.Select(p => p.ToInt()), writer);
            });
            WriteBDCEnum(GetData<Subject>(), writer, s =>
            {
                WriteIntEnum(s.Teachers.Select(t => t.StorageIndex), writer);
            });
            WriteBDCEnum(GetData<Form>(), writer, f =>
            {
                writer.Write(f.YearGroup.StorageIndex);
            });
            WriteBDCEnum(GetData<Room>(), writer, r =>
            {
                writer.Write(r.Quantity);
                writer.Write(r.Critical);
            });
            WriteBDCEnum(GetData<Group>(), writer, g =>
            {
                WriteIntEnum(g.Subjects.Select(s => s.StorageIndex), writer);
                WriteIntEnum(g.Rooms.Select(r => r.StorageIndex), writer);
            });
            WriteBDCEnum(GetData<Lesson>(), writer, l =>
            {
                WriteIntEnum(l.Forms.Select(f => f.StorageIndex), writer);
                writer.Write(l.LessonsPerCycle);
                writer.Write(l.LessonLength);
                writer.Write(l.Subject.StorageIndex);
                WriteList(l.Assignments, (a, i) =>
                {
                    writer.Write(a.Teacher.StorageIndex);
                    writer.Write(a.LessonCount);
                }, writer);
                
            });
            all_data.Seek(0, SeekOrigin.Begin);
            FileStream stream = File.OpenWrite(fpath);
            all_data.CopyTo(stream);
            stream.Close();
            writer.Close();
            all_data.Close();
        }
        public static string OpenFileDialogHelper()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Timetable files (*.TTBL)|*.TTBL|All files (*.*)|*.*",
                InitialDirectory = Properties.Settings.Default.LAST_ACCESSED_PATH,
                ValidateNames = true,
                DefaultExt = ".ttbl",
            };
            if (dialog.ShowDialog() == true)
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
            if (dialog.ShowDialog() == true)
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
            foreach (Window window in Application.Current.Windows)
            {
                window.Title = $"Timetabler - {fpath}";
            }
        }
    }

}
