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
    public class Lesson : BaseDataClass
    {
        private Form _form;
        public Form Form
        {
            get { return _form; }
            set
            {
                if (value != _form)
                {
                    _form?.Lessons.Remove(this);
                    _form = value;
                    value.Lessons.Add(this);
                    NotifyPropertyChanged("Form");
                }
            }
        }
        private int _lpc;
        public int LessonsPerCycle
        {
            get { return _lpc; }
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
            get { return _length; }
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
            get { return _subject; }
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
        public new void Unfreeze()
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
        public override void Commit()
        {
            base.Commit();
        }
    }
}