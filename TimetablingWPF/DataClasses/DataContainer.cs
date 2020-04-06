using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimetablingWPF
{
    public class DataContainer
    {
        public IList<StructureClasses.TimetableStructureWeek> TimetableStructure { get; private set; }
        public InternalObservableCollection<Teacher> Teachers { get; } = new InternalObservableCollection<Teacher>();
        public InternalObservableCollection<Form> Forms { get; } = new InternalObservableCollection<Form>();
        public InternalObservableCollection<Year> YearGroups { get; } = new InternalObservableCollection<Year>();
        public InternalObservableCollection<Lesson> Lessons { get; } = new InternalObservableCollection<Lesson>();
        public InternalObservableCollection<Room> Rooms { get; } = new InternalObservableCollection<Room>();
        public InternalObservableCollection<Subject> Subjects { get; } = new InternalObservableCollection<Subject>();
        public InternalObservableCollection<Group> Groups { get; } = new InternalObservableCollection<Group>();
        public bool Unsaved { get; private set; } = false;
        public DataContainer()
        {
            Teachers.CollectionChanged += SetUnsaved;
            Forms.CollectionChanged += SetUnsaved;
            YearGroups.CollectionChanged += SetUnsaved;
            Lessons.CollectionChanged += SetUnsaved;
            Rooms.CollectionChanged += SetUnsaved;
            Subjects.CollectionChanged += SetUnsaved;
            Groups.CollectionChanged += SetUnsaved;
        }
        private void SetUnsaved(object sender, NotifyCollectionChangedEventArgs e)
        {
            Unsaved = true;
            Teachers.CollectionChanged -= SetUnsaved;
            Forms.CollectionChanged -= SetUnsaved;
            YearGroups.CollectionChanged -= SetUnsaved;
            Lessons.CollectionChanged -= SetUnsaved;
            Rooms.CollectionChanged -= SetUnsaved;
            Subjects.CollectionChanged -= SetUnsaved;
            Groups.CollectionChanged -= SetUnsaved;
            FileHelpers.SetWindowHeaders();
        }
        public void UpdateSave()
        {
            Unsaved = false;
            Teachers.CollectionChanged += SetUnsaved;
            Forms.CollectionChanged += SetUnsaved;
            YearGroups.CollectionChanged += SetUnsaved;
            Lessons.CollectionChanged += SetUnsaved;
            Rooms.CollectionChanged += SetUnsaved;
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
            if (dataClass is Room room) Rooms.Add(room);
        }
        public IList FromType(Type type)
        {
            if (type == typeof(Teacher)) return Teachers;
            if (type == typeof(Form)) return Forms;
            if (type == typeof(Subject)) return Subjects;
            if (type == typeof(Lesson)) return Lessons;
            if (type == typeof(Group)) return Groups;
            if (type == typeof(Room)) return Rooms;
            if (type == typeof(Year)) return YearGroups;
            throw new ArgumentException("Type must be a legal type");
        }
        public void SetTimetableStructure(IList<StructureClasses.TimetableStructureWeek> weeks)
        {
            TimetableStructure = weeks;
        }
        public void SetFromContainer(DataContainer container)
        {
            TimetableStructure = container.TimetableStructure;
            Teachers.SetData(container.Teachers);
            Lessons.SetData(container.Lessons);
            Forms.SetData(container.Forms);
            Groups.SetData(container.Groups);
            Rooms.SetData(container.Rooms);
            Subjects.SetData(container.Subjects);
            YearGroups.SetData(container.YearGroups);
        }
        public void ClearData()
        {
            Teachers.Clear();
            Lessons.Clear();
            Forms.Clear();
            Groups.Clear();
            Rooms.Clear();
            Subjects.Clear();
            YearGroups.Clear();
        }
    }
}
