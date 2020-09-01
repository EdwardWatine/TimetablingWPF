using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimetablingWPF
{
    static class LocalSettings
    {
        public static int TeacherMaxPeriods { get; set; } = TimetableStructure.TotalSchedulable;
        public static int DelayBeforeSearching { get; set; } = 300; //millis
        public static int TooltipDelay { get; set; } = 300; //millis
        /**
         * Separator between constant and 'inherited' settings
         */
        public static DataProvider<int> RecentListSize { get; set; } = new DataProvider<int>(GlobalSettings.RecentListSize);
        public static DataProvider<int> AutosaveInterval { get; set; } = new DataProvider<int>(GlobalSettings.AutosaveInterval); //millis
        public static DataProvider<double> AnimationModifier { get; } = new DataProvider<double>(GlobalSettings.AnimationModifier);
        public static DataProvider<bool> AutoSubjectConstraint { get; } = new DataProvider<bool>(GlobalSettings.AutoSubjectConstraint);
    }
}
