﻿using Humanizer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace TimetablingWPF
{
    public class DataContainer : ICloneable
    {
        public IList<StructureClasses.TimetableStructureWeek> TimetableStructure { get; private set; }
        public InternalObservableCollection<Teacher> Teachers { get; } = new InternalObservableCollection<Teacher>();
        public InternalObservableCollection<Form> Forms { get; } = new InternalObservableCollection<Form>();
        public InternalObservableCollection<Year> YearGroups { get; } = new InternalObservableCollection<Year>();
        public InternalObservableCollection<Lesson> Lessons { get; } = new InternalObservableCollection<Lesson>();
        public InternalObservableCollection<Subject> Subjects { get; } = new InternalObservableCollection<Subject>();
        public InternalObservableCollection<Group> Groups { get; } = new InternalObservableCollection<Group>();
        public Timer Timer { get; private set; }
        private void Autosave(object sender, ElapsedEventArgs e)
        {
            Timer.Stop();
            string[] bks = FileHelpers.GetBackups(App.FilePath);
            string path;
            if (bks.Length == 0)
            {
                int prefix = 0;
                path = FileHelpers.MakeBackupPath(App.FilePath);
                while (File.Exists(path))
                {
                    path = $"{prefix}{FileHelpers.MakeBackupPath(App.FilePath)}";
                    prefix++;
                }
            }
            else
            {
                path = bks[0];
            }
            FileHelpers.WriteBackup(path);
            if (Unsaved)
            {
                Timer.Start();
            }
        }
        public void SetTimer()
        {
            Timer = new Timer(TimetableSettings.AutosaveInterval);
            Timer.Elapsed += Autosave;
            Timer.Stop();
        }
        public bool Unsaved { get; private set; } = false;
        public DataContainer()
        {
            Year none = new Year("None")
            {
                Visible = false
            };
            YearGroups.Add(none);
            Teachers.CollectionChanged += SetUnsaved;
            Forms.CollectionChanged += SetUnsaved;
            YearGroups.CollectionChanged += SetUnsaved;
            Lessons.CollectionChanged += SetUnsaved;
            Subjects.CollectionChanged += SetUnsaved;
            Groups.CollectionChanged += SetUnsaved;
        }
        private void SetUnsaved(object sender, NotifyCollectionChangedEventArgs e)
        {
            Unsaved = true;
            Timer?.Start();
            Teachers.CollectionChanged -= SetUnsaved;
            Forms.CollectionChanged -= SetUnsaved;
            YearGroups.CollectionChanged -= SetUnsaved;
            Lessons.CollectionChanged -= SetUnsaved;
            Subjects.CollectionChanged -= SetUnsaved;
            Groups.CollectionChanged -= SetUnsaved;
            FileHelpers.SetWindowHeaders();
        }
        public void UpdateSave()
        {
            Unsaved = false;
            Timer?.Stop();
            Teachers.CollectionChanged += SetUnsaved;
            Forms.CollectionChanged += SetUnsaved;
            YearGroups.CollectionChanged += SetUnsaved;
            Lessons.CollectionChanged += SetUnsaved;
            Subjects.CollectionChanged += SetUnsaved;
            Groups.CollectionChanged += SetUnsaved;
            FileHelpers.SetWindowHeaders();
        }
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
        public void SetTimetableStructure(IList<StructureClasses.TimetableStructureWeek> weeks)
        {
            TimetableStructure = weeks;
            TimetablingWPF.TimetableStructure.SetData(weeks);
            SetUnsaved(null, null);
        }
        public void SetFromContainer(DataContainer container)
        {
            TimetableStructure = container.TimetableStructure;
            Teachers.SetData(container.Teachers);
            Lessons.SetData(container.Lessons);
            Forms.SetData(container.Forms);
            Groups.SetData(container.Groups);
            Subjects.SetData(container.Subjects);
            YearGroups.SetData(container.YearGroups);
        }
        public void ClearData()
        {
            Teachers.Clear();
            Lessons.Clear();
            Forms.Clear();
            Groups.Clear();
            Subjects.Clear();
            YearGroups.Clear();
        }

        public object Clone()
        {
            DataContainer dc = new DataContainer()
            {
                TimetableStructure = new List<StructureClasses.TimetableStructureWeek>(TimetableStructure),
                Timer = null
            };
            dc.SetFromContainer(this);
            return dc;
        }
    }
}
