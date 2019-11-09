﻿using System;
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
    public class TimetableStructure
    {
        public TimetableStructure(int weeksPerCycle, IList<TimetableStructurePeriod> structure)
        {
            WeeksPerCycle = weeksPerCycle;
            Structure = structure;
            PeriodsPerDay = (from period in structure where period.IsSchedulable select period).Count();
            TotalFreePeriods = weeksPerCycle*5*PeriodsPerDay;
        }
        public int WeeksPerCycle { get; }
        public IList<TimetableStructurePeriod> Structure { get; private set; }
        public int TotalFreePeriods { get; }
        public int PeriodsPerDay { get; }
    }
    public struct TimetableStructurePeriod : IEquatable<TimetableStructurePeriod>
    {
        public TimetableStructurePeriod(string name, bool isSchedulable)
        {
            Name = name;
            IsSchedulable = isSchedulable;
        }
        public string Name { get; }
        public bool IsSchedulable { get; }

        public override bool Equals(object obj)
        {
            return obj is TimetableStructurePeriod tsp && Equals(tsp);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() + (IsSchedulable ? 0 : 17);
        }

        public static bool operator ==(TimetableStructurePeriod left, TimetableStructurePeriod right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TimetableStructurePeriod left, TimetableStructurePeriod right)
        {
            return !left.Equals(right);
        }

        public bool Equals(TimetableStructurePeriod other)
        {
            return Name == other.Name && IsSchedulable == other.IsSchedulable;
        }
    }

    public class YearGroup
    {
        public YearGroup(string year)
        {
            Year = year;
        }
        public string Year { get; set; }
        public int StorageIndex { get; set; }
        public override string ToString()
        {
            return $"Year {Year}";
        }

        public override bool Equals(object obj)
        {
            return obj is YearGroup yg && yg.Year == Year;
        }
        
        public override int GetHashCode()
        {
            return Year.GetHashCode();
        }

        public static bool operator ==(YearGroup left, YearGroup right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(YearGroup left, YearGroup right)
        {
            return !left.Equals(right);
        }
    }
}
