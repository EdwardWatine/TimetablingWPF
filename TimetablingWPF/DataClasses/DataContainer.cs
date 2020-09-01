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
using ObservableComputations;

namespace TimetablingWPF
{
    public class DataContainer
    {
        public IList<TimetableStructureWeek> TimetableStructure { get; private set; }
        public ObservableCollectionExtended<Teacher> Teachers { get; } = new ObservableCollectionExtended<Teacher>();
        public ObservableCollectionExtended<Form> Forms { get; } = new ObservableCollectionExtended<Form>();
        public ObservableCollectionExtended<Year> YearGroups { get; } = new ObservableCollectionExtended<Year>();
        public ObservableCollectionExtended<Lesson> Lessons { get; } = new ObservableCollectionExtended<Lesson>();
        public ObservableCollectionExtended<Subject> Subjects { get; } = new ObservableCollectionExtended<Subject>();
        public ObservableCollectionExtended<Group> Groups { get; } = new ObservableCollectionExtended<Group>();
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
        static SingletonDataContainer()
        {
            Instance.Initialise();
        }
        public SelectingMany<INotifyCollectionChanged, BaseDataClass> AllData { get; }
        public static SingletonDataContainer Instance { get; } = new SingletonDataContainer();
        public int NumErrors { get; private set; } = 0;
        public int NumWarnings { get; private set; } = 0;
        private static BackgroundTask BackupTask { get; } = new BackgroundTask("Backing up", "Backing up the data of the current application.");
        private Timer Timer;
        public delegate void SaveStateChangedHandler();
        public event SaveStateChangedHandler SaveStateChanged;
        public event ErrorStateChangedEventHandler ErrorStateChanged;

        public TimeSpan LastSave { get; private set; }
        public TimeSpan? LastBackup { get; private set; } = null;

        private void NotifyErrorStateChanged(ErrorStateChangedEventArgs e)
        {
            ErrorStateChanged?.Invoke(e.AppendObject(this));
        }
        public void Autosave()
        {
            Autosave(null, null);
        }
        private void Initialise()
        {
            Year none = new Year("None");
            YearGroups.Add(none);
            NoneYear = none;
            Subject noneS = new Subject()
            {
                Name = "None",
                Shorthand = "NONE",
                Visible = false
            };
            Subjects.Add(noneS);
            NoneSubject = noneS;
            AllData.CollectionChanged += SetUnsaved;
            YearGroups.CollectionChanged += SetUnsaved;
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
            Timer = new Timer(LocalSettings.AutosaveInterval.Value);
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
            AllData = new ObservableCollectionExtended<INotifyCollectionChanged>() { Teachers, Subjects, Lessons, Forms, Groups }.SelectingMany<INotifyCollectionChanged, BaseDataClass>(x => x);
        }
        private void SetUnsaved(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e != null && e.IsAddOrRemove() && ReferenceEquals(sender, AllData))
            {
                if (e.NewItems != null)
                {
                    foreach (object item in e.NewItems)
                    {
                        BaseDataClass obj = (BaseDataClass)item;
                        int errs = obj.GetErrorCount(ErrorType.Error);
                        NumErrors += errs;
                        if (errs > 0) NotifyErrorStateChanged(new ErrorStateChangedEventArgs(obj));
                        errs = obj.GetErrorCount(ErrorType.Warning);
                        NumWarnings += errs;
                        if (errs > 0) NotifyErrorStateChanged(new ErrorStateChangedEventArgs(obj));
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
                        if (errs > 0) NotifyErrorStateChanged(new ErrorStateChangedEventArgs(obj));
                        errs = obj.GetErrorCount(ErrorType.Warning);
                        NumWarnings -= errs;
                        if (errs > 0) NotifyErrorStateChanged(new ErrorStateChangedEventArgs(obj));
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

        private void ObjectErrorStateChanged(ErrorStateChangedEventArgs e)
        {
            ErrorContainer error = (ErrorContainer)e.ObjectChain[0];
            int change = error.IsErroredState ? 1 : -1;
            if (error.ErrorType == ErrorType.Error) NumErrors += change;
            if (error.ErrorType == ErrorType.Warning) NumWarnings += change;
            NotifyErrorStateChanged(e);
        }

        public void UpdateSave()
        {
            LastSave = DateTime.Now.TimeOfDay;
            Unsaved = false;
            SaveStateChanged?.Invoke();
            Timer?.Stop();
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
            Subjects.Add(NoneSubject);
        }
        public Year NoneYear { get; private set; }
        public Subject NoneSubject { get; private set; }
        public object ToContainer()
        {
            DataContainer dc = new DataContainer();
            dc.SetFromContainer(this);
            return dc;
        }
    }
}
