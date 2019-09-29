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
    public class TimetableStructure
    {
        public TimetableStructure(int weeksPerCycle, IList<TimetableStructurePeriod> structure)
        {
            WeeksPerCycle = weeksPerCycle;
            Structure = structure;
            TotalFreePeriods = weeksPerCycle*5*(from period in structure where period.IsSchedulable select period).Count();
        }
        public int WeeksPerCycle { get; }
        public IList<TimetableStructurePeriod> Structure { get; private set; }
        public int TotalFreePeriods { get; }
        public const string ListName = "Structure";
    }
    public struct TimetableStructurePeriod
    {
        public TimetableStructurePeriod(string name, bool isSchedulable)
        {
            Name = name;
            IsSchedulable = isSchedulable;
        }
        public string Name { get; }
        public bool IsSchedulable { get; }
    }
}
