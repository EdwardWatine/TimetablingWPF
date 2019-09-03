using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
using System.Diagnostics;

namespace TimetablingWPF
{
    public class RelationalList<T> : ObservableCollection<T>
    {
        private object Parent;
        public string ClassProperty { get; }
        private readonly string OtherClassProperty;

        public RelationalList(string classProperty, 
            string otherClassProperty, BaseDataClass parent = null)
        {
            ClassProperty = classProperty;
            OtherClassProperty = otherClassProperty;
            Parent = parent;
        }
        public void SetParent(object parent)
        {
            Parent = parent;
        }
        public void AddNoTrigger(T item)
        {
            base.Add(item);
        }
        public new void Add(T item)
        {
            base.Add(item);

            ((IList)item.GetType().GetProperty(OtherClassProperty).GetValue(item)).Add(Parent);
        }
    }

    public struct TimetableSlot
    {
        public int Week, Day, Period;
        public TimetableSlot(int week, int day, int period)
        {
            Week = week;
            Day = day;
            Period = period;
        }
    }

    public class Assignment
    {
        public Teacher Teacher;
        public Class Class;
        public int Periods;

        public Assignment(Teacher teacher, int periods)
        {
            Teacher = teacher;
            Periods = periods;
        }

        public Assignment(Class @class, int periods)
        {
            Class = @class;
            Periods = periods;
        }

        public void Commit(Teacher teacher)
        {
            if (Class == null)
            {
                throw new System.InvalidOperationException("Commit should be called with a class, as the class has not been set");
            }
            Teacher = teacher;
            Teacher.Assignments.Add(this);
            Class.Assignments.Add(this);
        }

        public void Commit(Class @class)
        {
            if (Teacher == null)
            {
                throw new System.InvalidOperationException("Commit should be called with a teacher, as the teacher has not been set");
            }
            Class = @class;
            Teacher.Assignments.Add(this);
            Class.Assignments.Add(this);
        }

        public override string ToString()
        {
            if (Class == null)
            {
                return $"{Teacher} ({Periods})";
            }
            if (Teacher == null)
            {
                return $"{Class} ({Periods})";
            }
            return $"{Teacher}: {Class} ({Periods})";
        }
    }

    public abstract class BaseDataClass : INotifyPropertyChanged
    {
        public string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        private string _Name;

        protected BaseDataClass()
        {
            string className = this.GetType().Name;
            ((IList)Application.Current.Properties[className.Pluralize()]).Add(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public override string ToString()
        {
            return Name;
        }
        public RelationalList<T> NewRL<T>(RelationalList<T> list)
        {
            if (list == null) { return null; }
            list.SetParent(this);
            return list;
        }
        public void Delete()
        {

        }
    }

    public class Room : BaseDataClass
    {
        public Room(string name, int quantity, RelationalList<Subject> subjects = null)
        {
            Name = name;
            Quantity = quantity;
            Subjects = NewRL(subjects) ?? new RelationalList<Subject>("Subjects", "Rooms", this);
        }
        private int _Quantity;
        public int Quantity
        {
            get { return _Quantity; }
            set
            {
                if (value != _Quantity)
                {
                    _Quantity = value;
                    NotifyPropertyChanged("Quantity");
                }
            }
        }
        public RelationalList<Subject> Subjects { get; }
    }

    public class Teacher : BaseDataClass
    {
        public Teacher(string name, ObservableCollection<TimetableSlot> unavailablePeriods,
            RelationalList<Subject> subjects = null, ObservableCollection<Assignment> assignments = null)
        {
            Name = name;
            UnavailablePeriods = unavailablePeriods;
            Subjects = NewRL(subjects) ?? new RelationalList<Subject>("Subjects", "Teachers", this);
            Assignments = assignments ?? new ObservableCollection<Assignment>();
            
        }
        public ObservableCollection<TimetableSlot> UnavailablePeriods { get; }
        public RelationalList<Subject> Subjects { get; }
        public ObservableCollection<Assignment> Assignments { get; }
    }

    public class Subject : BaseDataClass
    {
        public Subject(string name, RelationalList<Room> rooms = null, RelationalList<Teacher> teachers = null)
        {
            Name = name;
            Rooms = NewRL(rooms) ?? new RelationalList<Room>("Rooms", "Subjects", this);
            Teachers = NewRL(teachers) ?? new RelationalList<Teacher>("Teachers", "Subjects", this);
        }
        public RelationalList<Room> Rooms { get; }
        public RelationalList<Teacher> Teachers { get; }
    }

    public class Group : BaseDataClass
    {
        public Group(string name, RelationalList<Class> classes = null)
        {
            Name = name;
            Classes = NewRL(classes) ?? new RelationalList<Class>("Classes", "Groups", this);
        }
        public RelationalList<Class> Classes { get; }
    }

    public class Class : BaseDataClass
    {
        public Class(string name, Subject subject, int lessonsPerCycle,
            ObservableCollection<Assignment> assignments = null, RelationalList<Group> groups = null, int lessonLength = 1)
        {
            Name = name;
            Subject = subject;
            LessonsPerCycle = lessonsPerCycle;
            Assignments = assignments ?? 
                new ObservableCollection<Assignment>();
            Groups = NewRL(groups) ?? new RelationalList<Group>("Groups", "Classes", this);
            LessonLength = lessonLength;
        }
        private Subject _Subject;
        public Subject Subject
        {
            get { return _Subject; }
            set
            {
                if (value != _Subject)
                {
                    _Subject = value;
                    NotifyPropertyChanged("Subject");
                }
            }
        }
        private int _LessonsPerCycle;
        public int LessonsPerCycle
        {
            get { return _LessonsPerCycle; }
            set
            {
                if (value != _LessonsPerCycle)
                {
                    _LessonsPerCycle = value;
                    NotifyPropertyChanged("LessonsPerCycle");
                }
            }
        }
        public ObservableCollection<Assignment> Assignments { get; }
        public RelationalList<Group> Groups { get; }
        private int _LessonLength;
        public int LessonLength
        {
            get { return _LessonLength; }
            set
            {
                if (value != _LessonLength)
                {
                    _LessonLength = value;
                    NotifyPropertyChanged("LessonLength");
                }
            }
        }

    }
    public class TimetableStructure
    {
        public TimetableStructure(int weeksPerCycle, IList<TimetableStructurePeriod> structure)
        {
            WeeksPerCycle = weeksPerCycle;
            Structure = structure;
        }
        public int WeeksPerCycle { get; }
        public IList<TimetableStructurePeriod> Structure { get; }
    }
    public class TimetableStructurePeriod :  INotifyPropertyChanged
    {
        public TimetableStructurePeriod(string name, bool isSchedulable)
        {
            Name = name;
            IsSchedulable = isSchedulable;
        }
        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }
        private bool _IsSchedulable;
        public bool IsSchedulable
        {
            get { return _IsSchedulable; }
            set
            {
                if (value != _IsSchedulable)
                {
                    _IsSchedulable = value;
                    NotifyPropertyChanged("IsSchedulable");
                }
            }
        }
        public void NotifyPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
