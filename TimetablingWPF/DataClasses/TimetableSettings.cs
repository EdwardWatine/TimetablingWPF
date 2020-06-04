using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimetablingWPF
{
    static class TimetableSettings
    {
        public static int TeacherMaxPeriods { get; set; } = TimetableStructure.TotalSchedulable;
        public static int DelayBeforeSearching { get; set; } = 300;
        public static int RecentListSize { get; set; } = 6;
        public static int AutosaveInterval { get; set; } = 10000;
        public static int TooltipDelay { get; set; } = 300;
    }
}
