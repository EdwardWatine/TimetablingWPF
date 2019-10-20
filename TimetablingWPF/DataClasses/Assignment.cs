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
    public struct TimetableSlot : IEquatable<TimetableSlot>
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
            return $"{DataHelpers.WeekToString(Week)} {DataHelpers.DayToString(Day)} P{DataHelpers.PeriodNumToPeriod(Period).Name}";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TimetableSlot))
            {
                return false;
            }
            TimetableSlot slot = (TimetableSlot)obj;
            return Equals(slot);
        }

        public override int GetHashCode()
        {
            return new Tuple<int, int, int>(Week, Day, Period).GetHashCode();
        }

        public static bool operator ==(TimetableSlot left, TimetableSlot right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TimetableSlot left, TimetableSlot right)
        {
            return !(left == right);
        }

        public bool Equals(TimetableSlot slot)
        {
            return slot.Week == Week && slot.Day == Day && slot.Period == Period;
        }
    }
    /// <summary>
    /// Represents an assignment between a class and a teacher
    /// </summary>
    public class Assignment
    {
        public Teacher Teacher { get; private set; }
        public Band Class { get; private set; }
        public int Periods { get; }

        /// <summary>
        /// The string representation of this object from a teacher's perspective
        /// </summary>
        public string TeacherString { get; }
        /// <summary>
        /// The string representation of this object from a class' perspective
        /// </summary>
        public string ClassString { get; }
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
        public Assignment(Band band, int periods)
        {
            Class = band;
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
        /// <param name="band"></param>
        /// <exception cref="InvalidOperationException">This will be thrown if the class is already defined</exception>
        public void Commit(Band band)
        {
            if (Teacher == null)
            {
                throw new InvalidOperationException("Commit should be called with a teacher, as the teacher has not been set");
            }
            Class = band;
            Teacher.Assignments.Add(this);
            Class.Assignments.Add(this);
        }

        public new string ToString()
        {
            return $"{Teacher}: {Class} ({Periods})";
        }
    }
}