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
        public Lesson(Form form, Subject subject, int numLessons, int length)
        {
            Form = form;
            LessonsPerCycle = numLessons;
            LessonLength = length;
            Subject = subject;
        }
        public Form Form { get; }
        public int LessonsPerCycle { get; }
        public int LessonLength { get; }
        public Subject Subject { get; }
        public ObservableCollection<Assignment> Assignments { get; private set; } = new ObservableCollection<Assignment>();
        public const string ListName = "Lessons";
        protected override string ListNameAbstract => ListName;

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