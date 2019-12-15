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
using System.IO;

namespace TimetablingWPF
{
    public static class TimetableStructure
    {
        public static void SetData(int weeksPerCycle, IList<TimetableStructurePeriod> structure)
        {
            WeeksPerCycle = weeksPerCycle;
            Structure = structure;
            PeriodsPerDay = (from period in structure where period.IsSchedulable select period).Count();
            TotalFreePeriods = weeksPerCycle*5*PeriodsPerDay;
        }
        public static int WeeksPerCycle { get; private set; }
        public static IList<TimetableStructurePeriod> Structure { get; private set; }
        public static int TotalFreePeriods { get; private set; }
        public static int PeriodsPerDay { get; private set; }
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
        private bool Committed;
        public InternalObservableCollection<Form> Forms { get; private set; } = new InternalObservableCollection<Form>();
        public string Year { get; set; }
        public int StorageIndex { get; set; }
        public void Commit()
        {
            if (!Committed)
            {
                ((IList)Application.Current.Properties[typeof(YearGroup)]).Add(this);
                Committed = true;
            }
        }
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
