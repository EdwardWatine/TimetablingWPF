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
        public const string ListName = "Classes";
        public override string ListNameAbstract => ListName;
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
}