using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TimetablingWPF;
using TimetablingWPF.Searching;

namespace TimetablingWPF
{
    public class Teacher : BaseDataClass
    {
        static Teacher() // static constructor to expose properties
        {
            Type type = typeof(Teacher);
            RegisterProperty(type, nameof(UnavailablePeriods), "Unavailable Periods");
            RegisterProperty(type, nameof(MaxPeriodsPerCycle), "Maximum periods per cycle", o => $"{o}/{TimetableStructure.TotalSchedulable}");
            RegisterProperty(type, nameof(Subjects));
            RegisterProperty(type, nameof(Assignments));
            DataContainer current = App.Data;
            AddSearchParameter(type, new ListSearchFactory<Teacher, Group>(t => t.Subjects.SelectMany(s => s.Groups.Concat(new Group[] { s.RelatedGroup })), 
                current.Groups, "group", "Teaches"));
            AddSearchParameter(type, new ListSearchFactory<Teacher, Year>(t => t.Assignments.SelectMany(a => a.Lesson.Forms.Select(f => f.YearGroup)), current.YearGroups, "year", "Teaches"));
        }
        public Teacher()
        {
            Assignments.CollectionChanged += AssignmentsChanged;
            ErrorContainer no_periods = new ErrorContainer((e) => MaxPeriodsPerCycle == 0, (e) => "Teacher has no free periods.",
                ErrorType.Warning);
            no_periods.BindProperty(this, "MaxPeriodsPerCycle");
            ErrorList.Add(no_periods);

            ErrorContainer insuf_periods = new ErrorContainer((e) =>
            {
                int assigned = Assignments.Sum(a => a.TotalPeriods);
                e.Data = assigned;
                return MaxPeriodsPerCycle < assigned;
            },
                (e) => $"Teacher has fewer free periods ({MaxPeriodsPerCycle}) than assigned periods ({e.Data}).",
                ErrorType.Error);
            insuf_periods.BindCollection(UnavailablePeriods);
            insuf_periods.BindCollection(Assignments);
            ErrorList.Add(insuf_periods);

            ErrorContainer lesson_missing = new ErrorContainer((e) =>
            {
                IEnumerable<Assignment> assignmentMismatches = Assignments.Where(a => !Subjects.Contains(a.Lesson.Subject));
                e.Data = assignmentMismatches;
                return assignmentMismatches.Any();
            },
                (e) =>
                {
                    IEnumerable<Assignment> data = (IEnumerable<Assignment>)e.Data;
                    return $"The following Assignments have a subject that the teacher does not have: {string.Join(", ", data.Select(a => a.TeacherString))}.";
                },
                ErrorType.Warning);
            lesson_missing.BindCollection(Assignments);
            lesson_missing.BindCollection(Subjects);
            ErrorList.Add(lesson_missing);

            ErrorContainer insuf_lesson_slots = new ErrorContainer((e) =>
            {
                IEnumerable<Lesson> errors = Assignments.Where(a => a.Lesson.LessonsPerCycle <
                a.Lesson.Assignments.Where(a2 => (a2.Teacher ?? this) != this).Sum(a2 => a2.LessonCount) + a.LessonCount
                ).Select(a => a.Lesson);
                e.Data = errors;
                return errors.Any();
            },
                (e) =>
                {
                    IEnumerable<Lesson> errors = (IEnumerable<Lesson>)e.Data;
                    return $"The following lessons happen more frequently per cycle than has been assigned: {GenericHelpers.FormatEnumerable(errors)}.";
                },
                ErrorType.Warning);
            insuf_lesson_slots.BindCollection(Assignments);
            ErrorList.Add(lesson_missing);
            BindToErrors();
        }
        public ObservableCollection<TimetableSlot> UnavailablePeriods { get; private set; } = new ObservableCollection<TimetableSlot>();
        public RelationalCollection<Subject, Teacher> Subjects { get; private set; } = new RelationalCollection<Subject, Teacher>(nameof(Subject.Teachers));
        public ObservableCollection<Assignment> Assignments { get; private set; } = new ObservableCollection<Assignment>();
        private int maxppc = TimetableStructure.TotalSchedulable;
        public int AvailablePeriods => TimetableStructure.TotalSchedulable - UnavailablePeriods.Count;
        public int MaxPeriodsPerCycle
        {
            get => maxppc;
            set
            {
                if (value != maxppc)
                {
                    maxppc = value;
                    NotifyPropertyChanged(nameof(MaxPeriodsPerCycle));
                }
            }
        }
        public override void Delete(DataContainer dataContainer = null)
        {
            Assignments.Clear();
            base.Delete();
        }
        private readonly List<Assignment> frozenAssignmentsAdd = new List<Assignment>();
        private readonly List<Assignment> frozenAssignmentsRemove = new List<Assignment>();
        private void AssignmentsChanged(object sender, NotifyCollectionChangedEventArgs e) // intercept changes to assignment to deal with the frozen case
        {
            if (e.NewItems != null)
            {
                if (!Frozen)
                {
                    foreach (Assignment assignment in e.NewItems)
                    {
                        assignment.Lesson.Assignments.Add(assignment);
                    }
                }
                else
                {
                    frozenAssignmentsAdd.AddRange(e.NewItems.Cast<Assignment>());
                }
            }
            if (e.OldItems != null)
            {
                if (!Frozen)
                {
                    foreach (Assignment assignment in e.OldItems)
                    {
                        assignment.Lesson.Assignments.Remove(assignment);
                    }
                }
                else
                {
                    frozenAssignmentsRemove.AddRange(e.OldItems.Cast<Assignment>());
                }
            }
        }
        public override void Unfreeze()
        {
            base.Unfreeze();
            foreach (Assignment assignment in frozenAssignmentsAdd)
            {
                assignment.Lesson.Assignments.Add(assignment);
            }
            foreach (Assignment assignment in frozenAssignmentsRemove)
            {
                assignment.Lesson.Assignments.Remove(assignment);
            }
        }

        public override void Save(BinaryWriter writer)
        {
            SaveParent(writer);
            writer.Write(MaxPeriodsPerCycle);
            Saving.WriteIntEnum(UnavailablePeriods.Select(p => p.ToInt()), writer);
        }

        public override void Load(BinaryReader reader, Version version, DataContainer container)
        {
            LoadParent(reader, version, container);
            MaxPeriodsPerCycle = reader.ReadInt32();
            Loading.LoadEnum(() => UnavailablePeriods.Add(TimetableSlot.FromInt(reader.ReadInt32())), reader);
        }
    }
}
