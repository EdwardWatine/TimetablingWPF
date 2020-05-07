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
using TimetablingWPF.StructureClasses;

namespace TimetablingWPF
{
    public static class TimetableStructure
    {
        public static void SetData(IList<TimetableStructureWeek> weeks)
        {
            Weeks = weeks;
            TotalSchedulable = 0;
            foreach (TimetableStructureWeek week in Weeks)
            {
                TotalSchedulable += week.TotalSchedulable;
            }
        }
        public static IList<TimetableStructureWeek> Weeks { get; private set; }
        public static int TotalSchedulable { get; private set; }
    }
    public class Year : INotifyPropertyChanged
    {
        public Year(string name)
        {
            Name = name;
        }
        public bool Committed { get; private set; } = false;

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public InternalObservableCollection<Form> Forms { get; private set; } = new InternalObservableCollection<Form>();
        private string _year;
        public string Name
        {
            get => _year; set
            {
                if (value != _year)
                {
                    NotifyPropertyChanged(nameof(Year));
                    _year = value;
                }
            }
        }
        public int StorageIndex { get; set; }
        public void Commit(DataContainer container = null)
        {
            if (!Committed)
            {
                (container ?? DataHelpers.GetDataContainer()).YearGroups.Add(this);
                Committed = true;
            }
        }
        public void Delete(DataContainer container = null)
        {
            foreach (Form form in Forms)
            {
                form.YearGroup = null;
            }
            (container ?? DataHelpers.GetDataContainer()).YearGroups.Remove(this);
        }
        public override string ToString()
        {
            return $"Year {Name}";
        }

        public override bool Equals(object obj)
        {
            return obj is Year yg && yg.Name == Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
        public static bool operator ==(Year left, Year right)
        {
            return ReferenceEquals(left, right);
        }

        public static bool operator !=(Year left, Year right)
        {
            return !(left == right);
        }
    }
}
namespace TimetablingWPF.StructureClasses{
    public class TimetableStructureWeek
    {
        public TimetableStructureWeek(string name, IList<string> days, IList<string> periods, IList<int> unschedulable)
        {
            Name = name;
            DayNames = days;
            PeriodNames = periods;
            TotalPeriods = periods.Count * days.Count;
            TotalSchedulable = TotalPeriods;
            UnavailablePeriods = unschedulable;
            for (int i = 0; i < DayNames.Count; i++)
            {
                DaySchedulable.Add(0);
            }
            for (int i = 0; i < TotalPeriods; i++)
            {
                AllPeriods.Add(true);
            }
            foreach (int index in unschedulable)
            {
                AllPeriods[index] = false;
                TotalSchedulable--;
                DaySchedulable[index / PeriodNames.Count]++;
            }
        }
        public bool PeriodIsSchedulable(int day, int period)
        {
            return AllPeriods[IndexesToPeriodNum(day, period, PeriodNames.Count)];
        }
        public bool DayIsSchedulable(int day)
        {
            return DaySchedulable[day] != PeriodNames.Count;
        }
        public static int IndexesToPeriodNum(int day, int period, int numPeriods)
        {
            return day * numPeriods + period;
        }
        public string Name { get; }
        public IList<string> DayNames { get; }
        public IList<string> PeriodNames { get; }
        public IList<int> UnavailablePeriods { get; }
        public IList<bool> AllPeriods { get; } = new List<bool>();
        private IList<int> DaySchedulable { get; } = new List<int>();
        public int TotalPeriods { get; }
        public int TotalSchedulable { get; }
    }
}
