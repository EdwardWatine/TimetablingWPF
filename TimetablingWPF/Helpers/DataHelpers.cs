using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TimetablingWPF
{
    public static class DataHelpers
    {
        public static readonly List<string> ShortenedDaysOfTheWeek = new List<string>() { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
        public static string WeekToString(int week)
        {
            return TimetableStructure.Weeks[week].Name;
        }
        public static string DayToString(int week, int day)
        {
            return TimetableStructure.Weeks[week].DayNames[day];
        }
        public static string PeriodToString(int week, int period)
        {
            return TimetableStructure.Weeks[week].PeriodNames[period];
        }
        public static IList<T> GetData<T>()
        {
            return (IList<T>)Application.Current.Properties[typeof(T)];
        }
        public static void ClearData()
        {
            ((IList)Application.Current.Properties[typeof(YearGroup)]).Clear();
            foreach (Type type in (IList<Type>)Application.Current.Properties["USER_TYPES"])
            {
                ((IList)Application.Current.Properties[type]).Clear();
            }
        }
    }
}
