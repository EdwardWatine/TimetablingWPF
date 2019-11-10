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
        public int Week { get; }
        public int Day { get; }
        public int Period { get; }
        public TimetableSlot(int week, int day, int period)
        {
            Week = week;
            Day = day;
            Period = period;
        }
        public int ToInt()
        {
            TimetableStructure structure = DataHelpers.GetTimetableStructure();
            return 5 * (structure.PeriodsPerDay * Week + Day) + Period;
        }
        public ushort ToUshort()
        {
            return (ushort)ToInt();
        }
        public static TimetableSlot FromInt(int from)
        {
            TimetableStructure structure = DataHelpers.GetTimetableStructure();
            return new TimetableSlot(from / (structure.PeriodsPerDay * 5),
                from % (structure.PeriodsPerDay * 5) / 5,
                from % (structure.PeriodsPerDay * 5) % 5);
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
    /// Represents an assignment between a lesson and a teacher
    /// </summary>
    public class Assignment
    {
        public Teacher Teacher { get; }
        public Lesson Lesson { get; }
        public int LessonCount { get; }

        /// <summary>
        /// The string representation of this object from a teacher's perspective
        /// </summary>
        public string TeacherString { get; }
        /// <summary>
        /// The string representation of this object from a lesson's perspective
        /// </summary>
        public string LessonString { get; }
        /// <summary>
        /// A constructor for when the lesson is not known
        /// </summary>
        /// <param name="teacher"></param>
        /// <param name="periods"></param>
        public Assignment(Teacher teacher, Lesson lesson, int periods)
        {
            Teacher = teacher;
            Lesson = lesson;
            LessonCount = periods;
            LessonString = $"{Teacher} ({LessonCount})";
            TeacherString = $"{Lesson} ({LessonCount})";
        }
        /// <summary>
        /// Creates references to this assignment in the teacher and lesson assignments list
        /// </summary>
        public override string ToString()
        {
            return $"{Teacher} - {Lesson} ({LessonCount})";
        }
    }
}