using System;

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
            for (week = 0; num >= TimetableStructure.Weeks[week].TotalPeriods; week++)
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
}