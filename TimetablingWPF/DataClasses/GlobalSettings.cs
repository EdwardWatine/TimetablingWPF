using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimetablingWPF
{
    public static class GlobalSettings
    {
        public static DataWrapper<int> RecentListSize { get; } = new DataWrapper<int>(6);
        public static DataWrapper<int> AutosaveInterval { get; } = new DataWrapper<int>(10_000);
        public static DataWrapper<double> AnimationModifier { get; } = new DataWrapper<double>(1);
        public static DataWrapper<bool> AutoSubjectConstraint { get; } = new DataWrapper<bool>(true);
    }
}
