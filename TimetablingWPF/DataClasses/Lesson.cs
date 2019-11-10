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
        public Subject Subject { get; }
        public ObservableCollection<Assignment> Assignments { get; private set; } = new ObservableCollection<Assignment>();
        public override void Commit()
        {
            foreach (Assignment assignment in Assignments)
            {
                assignment.Teacher.Assignments.Add(assignment);
            }
            base.Commit();
        }
    }
}