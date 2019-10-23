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
            return !left.Equals(right);
        }

        public bool Equals(TimetableSlot slot)
        {
            return slot.Week == Week && slot.Day == Day && slot.Period == Period;
        }
    }
    /// <summary>
    /// Represents an assignment between a form and a teacher
    /// </summary>
    public class Assignment
    {
        public Teacher Teacher { get; private set; }
        public Form Form { get; private set; }
        public int Periods { get; }

        /// <summary>
        /// The string representation of this object from a teacher's perspective
        /// </summary>
        public string TeacherString { get; }
        /// <summary>
        /// The string representation of this object from a form' perspective
        /// </summary>
        public string SetString { get; }
        /// <summary>
        /// A constructor for when the form is not known
        /// </summary>
        /// <param name="teacher"></param>
        /// <param name="periods"></param>
        public Assignment(Teacher teacher, int periods)
        {
            Teacher = teacher;
            Periods = periods;
            SetString = $"{Teacher}: {Periods}";
        }
        /// <summary>
        /// A constructor for when the teacher is not known
        /// </summary>
        /// <param name="form"></param>
        /// <param name="periods"></param>
        public Assignment(Form form, int periods)
        {
            Form = form;
            Periods = periods;
            TeacherString = $"{Form}: {Periods}";
        }
        /// <summary>
        /// Creates references to this assignment in the teacher and form assignments list
        /// </summary>
        /// <param name="teacher"></param>
        /// <exception cref="System.InvalidOperationException">This will be thrown if the teacher is already defined</exception>
        public void Commit(Teacher teacher)
        {
            if (Form == null)
            {
                throw new InvalidOperationException("Commit should be called with a form, as the form has not been form");
            }
            Teacher = teacher;
            Teacher.Assignments.Add(this);
            Form.Assignments.Add(this);
        }
        /// <summary>
        /// Creates references to this assignment in the teacher and form assignments list
        /// </summary>
        /// <param name="form"></param>
        /// <exception cref="InvalidOperationException">This will be thrown if the form is already defined</exception>
        public void Commit(Form form)
        {
            if (Teacher == null)
            {
                throw new InvalidOperationException("Commit should be called with a teacher, as the teacher has not been form");
            }
            Form = form;
            Teacher.Assignments.Add(this);
            Form.Assignments.Add(this);
        }

        public new string ToString()
        {
            return $"{Teacher}: {Form} ({Periods})";
        }
    }
}