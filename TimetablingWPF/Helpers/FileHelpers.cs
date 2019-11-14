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
        private static void WriteIntEnum(IEnumerable<int> enumerable, BinaryWriter writer)
        {
            IList<int> list = enumerable.ToList();
            writer.Write(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                writer.Write(list[i]);
            }
        }
        private static void WriteBDCEnum<T>(IEnumerable<T> enumerable, BinaryWriter writer, Action<T> action) where T : BaseDataClass
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
        private static void LoadEnum(Action action, BinaryReader reader)
        {
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                action();
            }
        }
        private static void LoadBDCEnum<T>(Action<T> action, BinaryReader reader) where T : BaseDataClass, new()
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
        public static bool LoadData(string fpath, Action done = null, Window owner = null)
        {
            BackgroundWorker worker = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            LoadingBox box = new LoadingBox("Loading...")
            {
                Owner = owner
            };
            worker.DoWork += delegate (object sender, DoWorkEventArgs e) { 
                LoadDataFromFile(fpath, worker); };
            worker.ProgressChanged += delegate (object sender, ProgressChangedEventArgs e)
            {
                box.pbProgressBar.Value = e.ProgressPercentage;
                box.tbTextBlock.Text = (string)e.UserState;
            };
            worker.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs e) { box.Close(); done?.Invoke(); };
            worker.RunWorkerAsync();
            worker.Dispose();
            box.ShowDialog();
            return !worker.CancellationPending;
        }
        public static void LoadDataFromFile(string fpath, BackgroundWorker worker = null)
        {
            int total = 7;
            int count = 0;
            void UpdateWorker(string things)
            {
                worker?.ReportProgress(count * 100 / 7, $"Loading {things} ({count + 1}/{total})");
                count++;
            };
            UpdateWorker("Lessons");
            if (!File.Exists(fpath))
            {
                VisualHelpers.ShowErrorBox($"File path {fpath} does not exist.", "File not found!");
                RecentFilesRemove(fpath);
                worker.CancelAsync();
            }
            FileStream fileStream = File.OpenRead(fpath);
            BinaryReader reader = new BinaryReader(fileStream);
            LoadEnum(() =>
            {
                string name = reader.ReadString();
                YearGroup year = new YearGroup(name);
                year.Commit();
            }, reader);
            UpdateWorker("Teachers");
            LoadBDCEnum<Teacher>(t =>
            {
                LoadEnum(() => t.UnavailablePeriods.Add(TimetableSlot.FromInt(reader.ReadInt32())), reader);
            }, reader);
            UpdateWorker("Subjects");
            IList<Teacher> teacher_list = GetData<Teacher>();
            LoadBDCEnum<Subject>(s =>
            {
                LoadEnum(() => s.Teachers.Add(teacher_list[reader.ReadInt32()]), reader);
            }, reader);
            UpdateWorker("Forms");
            IList<YearGroup> year_list = GetData<YearGroup>();
            LoadBDCEnum<Form>(f =>
            {
                f.YearGroup = year_list[reader.ReadInt32()];
            }, reader);
            UpdateWorker("Rooms");
            LoadBDCEnum<Room>(r =>
            {
                r.Quantity = reader.ReadInt32();
                r.Critical = reader.ReadBoolean();
            }, reader);
            UpdateWorker("Groups");
            IList<Subject> subject_list = GetData<Subject>();
            IList<Room> room_list = GetData<Room>();
            LoadBDCEnum<Group>(g =>
            {
                LoadEnum(() => g.Subjects.Add(subject_list[reader.ReadInt32()]), reader);
                LoadEnum(() => g.Rooms.Add(room_list[reader.ReadInt32()]), reader);
            }, reader);
            UpdateWorker("Lessons");
            IList<Form> form_list = GetData<Form>();
            LoadBDCEnum<Lesson>(l =>
            {
                LoadEnum(() => l.Forms.Add(form_list[reader.ReadInt32()]), reader);
                l.LessonsPerCycle = reader.ReadInt32();
                l.LessonLength = reader.ReadInt32();
                l.Subject = subject_list[reader.ReadInt32()];
                LoadEnum(() => l.Assignments.Add(new Assignment(teacher_list[reader.ReadInt32()], l, reader.ReadInt32())), reader);
            }, reader);
            reader.Close();
            fileStream.Close();
        }
        public static void SaveDataToFile(string fpath)
        {
            MemoryStream all_data = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(all_data);
            IList<YearGroup> year_list = GetData<YearGroup>();
            writer.Write((int)year_list.Count);
            for (int i = 0; i < year_list.Count; i++)
            {
                YearGroup year = year_list[i];
                year.StorageIndex = i;
                writer.Write(year.Year);
            }
            WriteBDCEnum(GetData<Teacher>(), writer, t =>
            {
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
                writer.Write(l.Assignments.Count);
                foreach (Assignment assignment in l.Assignments)
                {
                    writer.Write(assignment.Teacher.StorageIndex);
                    writer.Write(assignment.LessonCount);
                }
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
                DefaultExt = ".ttbl"
            };
            if (dialog.ShowDialog() == true)
            {
                Properties.Settings.Default.LAST_ACCESSED_PATH = dialog.FileName;
                return dialog.FileName;
            }
            return null;
        }
        public static string SaveFileDialogHelper()
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "Timetable files (*.TTBL)|*.TTBL|All files (*.*)|*.*",
                InitialDirectory = Properties.Settings.Default.LAST_ACCESSED_PATH,
                ValidateNames = true,
                OverwritePrompt = true,
                DefaultExt = ".ttbl",
                AddExtension = true
            };
            if (dialog.ShowDialog() == true)
            {
                Properties.Settings.Default.LAST_ACCESSED_PATH = dialog.FileName;
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
