using Humanizer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Timers;
using TimetablingWPF.StructureClasses;

namespace TimetablingWPF
{
    public class DataContainer
    {
        public IList<TimetableStructureWeek> TimetableStructure { get; private set; }
        public InternalObservableCollection<Teacher> Teachers { get; } = new InternalObservableCollection<Teacher>();
        public InternalObservableCollection<Form> Forms { get; } = new InternalObservableCollection<Form>();
        public InternalObservableCollection<Year> YearGroups { get; } = new InternalObservableCollection<Year>();
        public InternalObservableCollection<Lesson> Lessons { get; } = new InternalObservableCollection<Lesson>();
        public InternalObservableCollection<Subject> Subjects { get; } = new InternalObservableCollection<Subject>();
        public InternalObservableCollection<Group> Groups { get; } = new InternalObservableCollection<Group>();
        public void AddFromBDC(BaseDataClass dataClass)
        {
            if (dataClass is Teacher teacher) Teachers.Add(teacher);
            if (dataClass is Form form) Forms.Add(form);
            if (dataClass is Subject subject) Subjects.Add(subject);
            if (dataClass is Lesson lesson) Lessons.Add(lesson);
            if (dataClass is Group group) Groups.Add(group);
        }
        public IList FromType(Type type)
        {
            if (type == typeof(Teacher)) return Teachers;
            if (type == typeof(Form)) return Forms;
            if (type == typeof(Subject)) return Subjects;
            if (type == typeof(Lesson)) return Lessons;
            if (type == typeof(Group)) return Groups;
            if (type == typeof(Year)) return YearGroups;
            throw new ArgumentException("Type must be a legal type");
        }
        public virtual void SetTimetableStructure(IList<TimetableStructureWeek> weeks)
        {
            TimetableStructure = weeks;
        }
        public void SetTimetableStructureAndHeaders(IList<TimetableStructureWeek> weeks)
        {
            SetTimetableStructure(weeks);
            TimetablingWPF.TimetableStructure.SetData(weeks);
        }
        public void SetFromContainer(DataContainer container)
        {
            SetTimetableStructure(container.TimetableStructure);
            Teachers.SetData(container.Teachers);
            Lessons.SetData(container.Lessons);
            Forms.SetData(container.Forms);
            Groups.SetData(container.Groups);
            Subjects.SetData(container.Subjects);
            YearGroups.SetData(container.YearGroups);
        }
    }
    public sealed class SingletonDataContainer : DataContainer, INotifyErrorStateChanged
    {
        public static SingletonDataContainer Instance { get; } = new SingletonDataContainer();
        public int NumErrors { get; private set; } = 0;
        public int NumWarnings { get; private set; } = 0;
        public static BackgroundTask BackupTask { get; } = new BackgroundTask("Backing up", "Backing up the data of the current application.");
        private Timer Timer;
        public delegate void SaveStateChangedHandler();
        public event SaveStateChangedHandler SaveStateChanged;
        public event ErrorStateChangedEventHandler ErrorStateChanged;

        public TimeSpan LastSave { get; private set; }
        public TimeSpan? LastBackup { get; private set; } = null;
        private readonly IList<INotifyCollectionChanged> DataLists;

        private void NotifyErrorStateChanged(ErrorStateChangedEventArgs e)
        {
            ErrorStateChanged?.Invoke(this, e);
        }
        public void Autosave()
        {
            Autosave(null, null);
        }
        public void Autosave(object sender, ElapsedEventArgs e)
        {
            Timer.Stop();
            BackgroundTaskManager.Tasks.Add(BackupTask);
            new System.Threading.Thread(new System.Threading.ThreadStart(() =>
            {
                IEnumerable<string> bks = FileHelpers.GetBackups(App.FilePath);
                string path;
                if (!bks.Any())
                {
                    int prefix = 0;
                    path = FileHelpers.MakeBackupPath(App.FilePath);
                    string fname = Path.GetFileName(path);
                    string direc = Path.GetDirectoryName(path);
                    while (File.Exists(path) && FileHelpers.ReadBackupPath(path) != App.FilePath)
                    {
                        path = Path.Combine(direc, $"{prefix}{fname}");
                        prefix++;
                    }
                }
                else
                {
                    path = bks.First();
                }
                FileHelpers.WriteBackup(path);
                LastBackup = DateTime.Now.TimeOfDay;
                BackgroundTaskManager.Tasks.Remove(BackupTask);
                SaveStateChanged?.Invoke();
                if (Unsaved)
                {
                    Timer.Start();
                }
            })).Start();
        }
        public void SetTimer()
        {
            Timer = new Timer(TimetableSettings.AutosaveInterval);
            Timer.Elapsed += Autosave;
            Timer.Stop();
        }
        public override void SetTimetableStructure(IList<TimetableStructureWeek> weeks)
        {
            base.SetTimetableStructure(weeks);
            SetUnsaved(null, null);
        }
        public bool Unsaved { get; private set; } = false;
        private SingletonDataContainer()
        {
            Year none = new Year("None")
            {
                Visible = false
            };
            YearGroups.Add(none);
            NoneYear = none;
            DataLists = new List<INotifyCollectionChanged>()
            {
                Teachers, Lessons, Subjects, Groups, Forms
            };
            foreach (INotifyCollectionChanged collection in DataLists)
            {
                collection.CollectionChanged += SetUnsaved;
            }
        }
        private void SetUnsaved(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e != null && e.IsNotPropertyChanged())
            {
                if (e.NewItems != null)
                {
                    foreach (object item in e.NewItems)
                    {
                        BaseDataClass obj = (BaseDataClass)item;
                        int errs = obj.GetErrorCount(ErrorType.Error);
                        NumErrors += errs;
                        if (errs > 0) NotifyErrorStateChanged(new ErrorStateChangedEventArgs(true, ErrorType.Error));
                        errs = obj.GetErrorCount(ErrorType.Warning);
                        NumWarnings += errs;
                        if (errs > 0) NotifyErrorStateChanged(new ErrorStateChangedEventArgs(true, ErrorType.Warning));
                        obj.ErrorStateChanged += ObjectErrorStateChanged;
                    }
                }
                if (e.OldItems != null)
                {
                    foreach (object item in e.OldItems)
                    {
                        BaseDataClass obj = (BaseDataClass)item;
                        int errs = obj.GetErrorCount(ErrorType.Error);
                        NumErrors -= errs;
                        if (errs > 0) NotifyErrorStateChanged(new ErrorStateChangedEventArgs(false, ErrorType.Error));
                        errs = obj.GetErrorCount(ErrorType.Warning);
                        NumWarnings -= errs;
                        if (errs > 0) NotifyErrorStateChanged(new ErrorStateChangedEventArgs(false, ErrorType.Warning));
                        obj.ErrorStateChanged -= ObjectErrorStateChanged;
                    }
                }
            }
            if (!Unsaved)
            {
                Unsaved = true;
                SaveStateChanged?.Invoke();
                Timer?.Start();
                FileHelpers.SetWindowHeaders();
            }
        }

        private void ObjectErrorStateChanged(object sender, ErrorStateChangedEventArgs e)
        {
            int change = e.ErrorState ? 1 : -1;
            if (e.ErrorType == ErrorType.Error) NumErrors += change;
            if (e.ErrorType == ErrorType.Warning) NumWarnings += change;
            NotifyErrorStateChanged(e);
        }

        public void UpdateSave()
        {
            LastSave = DateTime.Now.TimeOfDay;
            Unsaved = false;
            SaveStateChanged?.Invoke();
            Timer?.Stop();
            Teachers.CollectionChanged += SetUnsaved;
            Forms.CollectionChanged += SetUnsaved;
            YearGroups.CollectionChanged += SetUnsaved;
            Lessons.CollectionChanged += SetUnsaved;
            Subjects.CollectionChanged += SetUnsaved;
            Groups.CollectionChanged += SetUnsaved;
            FileHelpers.SetWindowHeaders();
        }
        public void ClearData()
        {
            Teachers.Clear();
            Lessons.Clear();
            Forms.Clear();
            Groups.Clear();
            Subjects.Clear();
            YearGroups.Clear();
            YearGroups.Add(NoneYear);
        }
        public Year NoneYear { get; private set; }
        public object ToContainer()
        {
            DataContainer dc = new DataContainer();
            dc.SetFromContainer(this);
            return dc;
        }
    }
}
