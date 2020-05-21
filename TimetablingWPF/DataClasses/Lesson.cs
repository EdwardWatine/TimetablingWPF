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
using System.IO;
using TimetablingWPF.Errors;

namespace TimetablingWPF
{
    public class Lesson : BaseDataClass
    {
        static Lesson()
        {
            Type type = typeof(Lesson);
            RegisterProperty(type, nameof(Forms));
            RegisterProperty(type, nameof(LessonsPerCycle));
            RegisterProperty(type, nameof(LessonLength));
            RegisterProperty(type, nameof(Subject));
            RegisterProperty(type, nameof(Assignments));
        }
        public Lesson()
        {
            Assignments.CollectionChanged += AssignmentsChanged;
            ErrorContainer no_subject = new ErrorContainer((e) => Subject == null, (e) => "No subject has been selected.", ErrorType.Error);
            no_subject.BindProperty(this, nameof(Subject));

            ErrorContainer too_many_lessons = new ErrorContainer(e => LessonLength * LessonsPerCycle > TimetableStructure.TotalSchedulable, e => $"This lesson has a minimum of {LessonsPerCycle * LessonLength} periods per cycle, but there is a maximum of {TimetableStructure.TotalSchedulable} periods per cycle.",
                ErrorType.Error);
            too_many_lessons.BindProperty(this, nameof(LessonLength));
            too_many_lessons.BindProperty(this, nameof(LessonsPerCycle));

            ErrorContainer too_many_assigned = new ErrorContainer(e =>
            {
                int total = Assignments.Sum(a => a.LessonCount);
                e.Data = total;
                return total > LessonsPerCycle;
            },
                e =>
                {
                    int total = (int)e.Data;
                    return $"This lesson has {total} lessons assigned, but there is supposed to be {LessonsPerCycle}";
                },
                ErrorType.Warning);
            too_many_assigned.BindProperty(this, nameof(LessonsPerCycle));
            too_many_assigned.BindCollection(Assignments);

            ErrorContainer insuf_teacher_slots = new ErrorContainer((e) =>
            {
                IEnumerable<Teacher> errors = Assignments.Where(a => a.Teacher.MaxPeriodsPerCycle <
                a.Teacher.Assignments.Where(a2 => (a2.Lesson ?? this) != this).Sum(a2 => a2.TotalPeriods) + a.TotalPeriods).Select(a => a.Teacher);
                e.Data = errors;
                return errors.Any();
            },
                (e) =>
                {
                    IEnumerable<Lesson> errors = (IEnumerable<Lesson>)e.Data;
                    return $"The following teachers have more periods assigned to them than they have available: {GenericHelpers.FormatEnumerable(errors)}.";
                },
                ErrorType.Warning);
            insuf_teacher_slots.BindCollection(Assignments);
            insuf_teacher_slots.BindProperty(this, nameof(LessonLength));

            errorValidations = new List<ErrorContainer>()
            {
                no_subject, too_many_lessons, too_many_assigned, insuf_teacher_slots
            };
        }
        public RelationalCollection<Form, Lesson> Forms { get; private set; } = new RelationalCollection<Form, Lesson>(nameof(Form.Lessons));
        private int _lpc;
        public int LessonsPerCycle
        {
            get => _lpc;
            set
            {
                if (value != _lpc)
                {
                    _lpc = value;
                    NotifyPropertyChanged(nameof(LessonsPerCycle));
                }
            }
        }
        private int _length;
        public int LessonLength
        {
            get => _length;
            set
            {
                if (value != _length)
                {
                    _length = value;
                    NotifyPropertyChanged(nameof(LessonsPerCycle));
                }
            }
        }
        private Subject _subject;
        public Subject Subject
        {
            get => _subject;
            set
            {
                if (value != _subject)
                {
                    _subject?.Lessons.Remove(this);
                    _subject = value;
                    value.Lessons.Add(this);
                }
            }
        }
        public override void Delete(DataContainer dataContainer = null)
        {
            Assignments.Clear();
            base.Delete();
        }
        public ObservableCollection<Assignment> Assignments { get; private set; } = new ObservableCollection<Assignment>();
        private readonly List<Assignment> frozenAssignmentsAdd = new List<Assignment>();
        private readonly List<Assignment> frozenAssignmentsRemove = new List<Assignment>();
        private void AssignmentsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                if (!Frozen)
                {
                    foreach (Assignment assignment in e.NewItems)
                    {
                        assignment.Teacher.Assignments.Add(assignment);
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
                        assignment.Teacher.Assignments.Remove(assignment);
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
                assignment.Teacher.Assignments.Add(assignment);
            }
            foreach (Assignment assignment in frozenAssignmentsRemove)
            {
                assignment.Teacher.Assignments.Remove(assignment);
            }
        }

        public override void Save(BinaryWriter writer)
        {
            SaveParent(writer);
            Saving.WriteIntEnum(Forms.Select(f => f.StorageIndex), writer);
            writer.Write(LessonsPerCycle);
            writer.Write(LessonLength);
            writer.Write(Subject.StorageIndex);
            Saving.WriteList(Assignments, (a, i) =>
            {
                writer.Write(a.Teacher.StorageIndex);
                writer.Write(a.LessonCount);
            }, writer);
        }

        public override void Load(BinaryReader reader, Version version, DataContainer container)
        {
            LoadParent(reader, version, container);
            Loading.LoadEnum(() => Forms.Add(container.Forms[reader.ReadInt32()]), reader);
            LessonsPerCycle = reader.ReadInt32();
            LessonLength = reader.ReadInt32();
            Subject = container.Subjects[reader.ReadInt32()];
            Loading.LoadEnum(() => Assignments.Add(new Assignment(container.Teachers[reader.ReadInt32()], this, reader.ReadInt32())), reader);
        }

        private readonly IList<ErrorContainer> errorValidations;
        public override IEnumerable<ErrorContainer> ErrorValidations => errorValidations;
    }
}
