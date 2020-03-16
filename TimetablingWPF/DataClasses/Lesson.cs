﻿using System;
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
    public class Lesson : BaseDataClass
    {
        static Lesson()
        {
            Type type = typeof(Lesson);
            RegisterProperty(type, "Forms");
            RegisterProperty(type, "LessonsPerCycle");
            RegisterProperty(type, "LessonLength");
            RegisterProperty(type, "Subject");
            RegisterProperty(type, "Assignments");
        }
        public Lesson()
        {
            Assignments.CollectionChanged += AssignmentsChanged;
        }
        public RelationalCollection<Form, Lesson> Forms { get; private set; } = new RelationalCollection<Form, Lesson>("Lessons");
        private int _lpc;
        public int LessonsPerCycle
        {
            get => _lpc;
            set
            {
                if (value != _lpc)
                {
                    _lpc = value;
                    NotifyPropertyChanged("LessonsPerCycle");
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
                    NotifyPropertyChanged("LessonsPerCycle");
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
    }
}
