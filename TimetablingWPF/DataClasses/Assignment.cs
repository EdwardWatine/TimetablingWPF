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
        public override string ToString()
        {
            return $"{DataHelpers.WeekToString(Week)} {DataHelpers.DayToString(Week, Day)} {DataHelpers.PeriodToString(Week, Period)}";
        }
        public int ToInt()
        {
            return ToIntFromInts(Week, Day, Period);
        }
        public static int ToIntFromInts(int week, int day, int period)
        {
            int total = 0;
            for (int i = 0; i < week; i++)
            {
                total += TimetableStructure.Weeks[i].TotalPeriods;
            }
            total += day * TimetableStructure.Weeks[week].PeriodNames.Count;
            total += period;
            return total;
        }

        public static TimetableSlot FromInt(int num)
        {
            int week;
            for (week = 0; num <= TimetableStructure.Weeks[week].TotalPeriods; week++)
            {
                num -= TimetableStructure.Weeks[week].TotalPeriods;
            }
            int day = num / TimetableStructure.Weeks[week].PeriodNames.Count;
            int period = num % TimetableStructure.Weeks[week].PeriodNames.Count;
            return new TimetableSlot(week, day, period);
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
            return ToInt();
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