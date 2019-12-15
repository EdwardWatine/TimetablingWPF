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
    class DataHelpers
    {
        public static string WeekToString(int week)
        {
            return Convert.ToString((char)('A' + week));
        }
        public static string DayToString(int day)
        {
            string[] days = { "Mon", "Tue", "Wed", "Thu", "Fri" };
            return days[day];
        }
        public static TimetableStructurePeriod PeriodNumToPeriod(int period)
        {
            return TimetableStructure.Structure[period];
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
