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
using System.Collections.Specialized;

namespace TimetablingWPF
{
    /// <summary>
    /// Holds data about a timetabling slot
    /// </summary>
    public struct TimetableSlot
    {
        public int Week, Day, Period;
        public TimetableSlot(int week, int day, int period)
        {
            Week = week;
            Day = day;
            Period = period;
        }
        public override string ToString()
        {
            ;
            TimetableStructure structure = (TimetableStructure)Application.Current.Properties["Structure"];
            return $"{Utility.WeekToString(Week)} {Utility.DayToString(Day)} P{Utility.PeriodNumToPeriod(Period).Name}";
        }
    }
    /// <summary>
    /// Represents an assignment between a class and a teacher
    /// </summary>
    public class Assignment
    {
        public Teacher Teacher;
        public Class Class;
        public int Periods;
        /// <summary>
        /// The string representation of this object from a teacher's perspective
        /// </summary>
        public string TeacherString;
        /// <summary>
        /// The string representation of this object from a class' perspective
        /// </summary>
        public string ClassString;
        /// <summary>
        /// A constructor for when the class is not known
        /// </summary>
        /// <param name="teacher"></param>
        /// <param name="periods"></param>
        public Assignment(Teacher teacher, int periods)
        {
            Teacher = teacher;
            Periods = periods;
            ClassString = $"{Teacher}: {Periods}";
        }
        /// <summary>
        /// A constructor for when the teacher is not known
        /// </summary>
        /// <param name="class"></param>
        /// <param name="periods"></param>
        public Assignment(Class @class, int periods)
        {
            Class = @class;
            Periods = periods;
            TeacherString = $"{Class}: {Periods}";
        }
        /// <summary>
        /// Creates references to this assignment in the teacher and class assignments list
        /// </summary>
        /// <param name="teacher"></param>
        /// <exception cref="System.InvalidOperationException">This will be thrown if the teacher is already defined</exception>
        public void Commit(Teacher teacher)
        {
            if (Class == null)
            {
                throw new InvalidOperationException("Commit should be called with a class, as the class has not been set");
            }
            Teacher = teacher;
            Teacher.Assignments.Add(this);
            Class.Assignments.Add(this);
        }
        /// <summary>
        /// Creates references to this assignment in the teacher and class assignments list
        /// </summary>
        /// <param name="@class"></param>
        /// <exception cref="InvalidOperationException">This will be thrown if the class is already defined</exception>
        public void Commit(Class @class)
        {
            if (Teacher == null)
            {
                throw new InvalidOperationException("Commit should be called with a teacher, as the teacher has not been set");
            }
            Class = @class;
            Teacher.Assignments.Add(this);
            Class.Assignments.Add(this);
        }

        public new string ToString()
        {
            return $"{Teacher}: {Class} ({Periods})";
        }
    }
    /// <summary>
    /// Base class for all data objects
    /// </summary>
    public abstract class BaseDataClass : INotifyPropertyChanged, ICloneable
    {

        public BaseDataClass()
        {
            ApplyOnType<IRelationalList>((prop, val) => val.Parent = this);
            void SubscribeToCollectionChange(System.Reflection.PropertyInfo prop, INotifyCollectionChanged val)
            {
                void Val_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
                {
                    if (e.Action != NotifyCollectionChangedAction.Replace)
                    {
                        NotifyPropertyChanged(prop.Name);
                        return;
                    }
                }
                val.CollectionChanged += Val_CollectionChanged;
            }
            

            ApplyOnType<INotifyCollectionChanged>(SubscribeToCollectionChange);
        }

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
        /// <summary>
        /// Holder for Name
        /// </summary>
        private string _Name;
        protected abstract string ListName { get; }
        private bool Commited = false;
        /// <summary>
        /// Add this to its associated list in properties. Is idempotent.
        /// </summary>
        public virtual void Commit()
        {
            if (Commited)
            {
                return;
            }
            ((IList)Application.Current.Properties[ListName]).Add(this);
            Commited = true;
        }
        public void Recommit(BaseDataClass replace)
        {
            int index = ((IList)Application.Current.Properties[ListName]).IndexOf(this);
            ((IList)Application.Current.Properties[ListName])[index] = replace;
            Delete();
        }
        /// <summary>
        /// Event when property is changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public override string ToString()
        {
            return Name;
        }
        /// <summary>
        /// Will remove all instances of self from <see cref="RelationalList{T}"/>. Will then remove self from the properties list
        /// </summary>
        public void Delete()
        {
            void delete(System.Reflection.PropertyInfo prop, IRelationalList val)
            {
                foreach (object @object in (IEnumerable)val)
                {
                    RelationalList<BaseDataClass> rlist = (RelationalList<BaseDataClass>)@object.GetType().GetProperty(val.OtherClassProperty).GetValue(@object);
                    rlist.Remove(this);
                }
            }
            ApplyOnType<IRelationalList>(delete);
            ((IList)Application.Current.Properties[ListName]).Remove(this);
        }

        public object Clone()
        {
            object copy = MemberwiseClone();
            ((BaseDataClass)copy).Commited = false;
            ApplyOnType<IEnumerable>((prop, val) => prop.SetValue(copy, ((ICloneable)val).Clone()));
            ApplyOnType<IRelationalList>((prop, val) => val.Parent = this);
            return copy;
        }

        private void ApplyOnType<T>(Action<System.Reflection.PropertyInfo, T> action)
        {
            foreach (System.Reflection.PropertyInfo prop in GetType().GetProperties())
            {
                object val = prop.GetValue(this);
                if (val is T)
                {
                    action(prop, (T)val);
                }
            };
        }
    }

    public class Room : BaseDataClass
    {
        public Room(string name, int quantity)
        {
            Name = name;
            Quantity = quantity;
            Subjects = new RelationalList<Subject>("Rooms", this);
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
        public RelationalList<Subject> Subjects { get; private set; }

        protected override string ListName => "Rooms";
    }

    public class Teacher : BaseDataClass
    {
        public ObservableCollection<TimetableSlot> UnavailablePeriods { get; private set; } = new ObservableCollection<TimetableSlot>();
        public RelationalList<Subject> Subjects { get; private set; } = new RelationalList<Subject>("Teachers");
        public ObservableCollection<Assignment> Assignments { get; private set; } = new ObservableCollection<Assignment>();
        protected override string ListName => "Teachers";
    }

    public class Subject : BaseDataClass
    {
        public RelationalList<Room> Rooms { get; private set; } = new RelationalList<Room>("Subjects");
        public RelationalList<Teacher> Teachers { get; private set; } = new RelationalList<Teacher>("Subjects");
        protected override string ListName => "Subjects";
    }

    public class Group : BaseDataClass
    {
        public RelationalList<Class> Classes { get; private set; } = new RelationalList<Class>("Groups");
        protected override string ListName => "Groups";
    }

    public class Class : BaseDataClass
    {
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
        public ObservableCollection<Assignment> Assignments { get; private set; } = new ObservableCollection<Assignment>();
        public RelationalList<Group> Groups { get; private set; } = new RelationalList<Group>("Classes");
        protected override string ListName => "Classes";
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
            TotalFreePeriods = weeksPerCycle*5*(from period in structure where period.IsSchedulable select period).Count();
        }
        public int WeeksPerCycle { get; }
        public IList<TimetableStructurePeriod> Structure { get; private set; }
        public int TotalFreePeriods { get; }
    }
    public struct TimetableStructurePeriod
    {
        public TimetableStructurePeriod(string name, bool isSchedulable)
        {
            Name = name;
            IsSchedulable = isSchedulable;
        }
        public string Name { get; }
        public bool IsSchedulable { get; }
    }
}
