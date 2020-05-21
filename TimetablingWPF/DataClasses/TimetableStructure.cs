using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
using System.Diagnostics;
using System.Collections.Specialized;
using System.IO;
using TimetablingWPF.StructureClasses;
using static TimetablingWPF.FileHelpers;

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

        public static void Save(BinaryWriter writer)
        {
            Saving.WriteList(Weeks, (w, i) =>
            {
                writer.Write(w.Name);
                Saving.WriteList(w.DayNames, (d, i2) =>
                {
                    writer.Write(d);
                }, writer);
                Saving.WriteList(w.PeriodNames, (p, i2) =>
                {
                    writer.Write(p);
                }, writer);
                Saving.WriteIntEnum(w.UnavailablePeriods, writer);
            }, writer);
        }

        public static void Load(BinaryReader reader, Version version, DataContainer container)
        {
            
            container.SetTimetableStructure(
                Loading.LoadAndReturnList(() =>
                {
                    return new TimetableStructureWeek(reader.ReadString(),
                        Loading.LoadAndReturnList(() =>
                        {
                            return reader.ReadString();
                        }, reader),
                        Loading.LoadAndReturnList(() =>
                        {
                            return reader.ReadString();
                        }, reader),
                        Loading.LoadAndReturnIntList(reader)
                        );
                }, reader)
            );
        }

        public static IList<TimetableStructureWeek> Weeks { get; private set; }
        public static int TotalSchedulable { get; private set; }
    }
}
namespace TimetablingWPF.StructureClasses
{
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
