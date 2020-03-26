﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace TimetablingWPF
{
    public class Teacher : BaseDataClass
    {
        static Teacher() // static constructor to expose properties
        {
            Type type = typeof(Teacher);
            RegisterProperty(type, "UnavailablePeriods", "Unavailable Periods");
            RegisterProperty(type, "MaxPeriodsPerCycle", "Maximum periods per cycle", o => $"{o}/{TimetableStructure.TotalSchedulable}");
            RegisterProperty(type, "Subjects");
            RegisterProperty(type, "Assignments");
        }
        public Teacher()
        {
            Assignments.CollectionChanged += AssignmentsChanged;
        }
        public ObservableCollection<TimetableSlot> UnavailablePeriods { get; private set; } = new ObservableCollection<TimetableSlot>();
        public RelationalCollection<Subject, Teacher> Subjects { get; private set; } = new RelationalCollection<Subject, Teacher>("Teachers");
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
                    NotifyPropertyChanged("MaxPeriodsPerCycle");
                }
            }
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
    }
}
