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
            TimetableStructure structure = (TimetableStructure)Application.Current.Properties[TimetableStructure.ListName];
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
}