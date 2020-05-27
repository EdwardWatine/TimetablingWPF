using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TimetablingWPF
{
    public static class DataHelpers
    {
        public static readonly string[] ShortenedDaysOfTheWeek = new string[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
        public static readonly Type[] UserTypes = new Type[] { typeof(Teacher), typeof(Subject), typeof(Form), typeof(Room), typeof(Group), typeof(Lesson) };
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
        public static DataContainer GetDataContainer()
        {
            return (DataContainer)Application.Current.Properties["CURRENT_DATA"];
        }
        public static void SetDataContainer(DataContainer container)
        {
            TimetableStructure.SetData(container.TimetableStructure);
            GetDataContainer().SetFromContainer(container);
        }
        public static void ClearData()
        {
            GetDataContainer().ClearData();
        }
        public static TabItem GenerateItemTab(object item, CommandType commandType)
        {
            return new ItemTab((BaseDataClass)item, commandType);
        }
        public static Predicate<object> GenerateDefaultNameFilter(string nameFilter, string shorthand)
        {
            return new Predicate<object>(o =>
            {
                BaseDataClass obj = (BaseDataClass)o;
                string name = obj.Name.RemoveWhitespace().ToUpperInvariant();
                string sh = obj.Shorthand.RemoveWhitespace().ToUpperInvariant();
                bool contains = name.Contains(nameFilter);
                if (nameFilter.Length < name.Length)
                {
                    name = name.Substring(0, nameFilter.Length);
                }
                if (shorthand.Length < sh.Length)
                {
                    sh = sh.Substring(0, shorthand.Length);
                }
                if (nameFilter.Length == 0 && shorthand.Length == 0) return true;
                if (contains && name.Length != 0) return true;
                bool nameCheck = name.Length == 0 ? false : GenericHelpers.DamerauLevenshteinDistance(name, nameFilter, (nameFilter.Length + 1) / 2) != int.MaxValue;
                if (nameCheck) return true;
                bool shCheck = sh.Length == 0 ? false : GenericHelpers.DamerauLevenshteinDistance(sh, shorthand, (sh.Length - 1) / 2) != int.MaxValue;
                if (shCheck) return true;
                return false;
               ;
            });
        }
        public static Predicate<object> GenerateNameFilter(string nameFilter, Func<object, string> strfunc)
        {
            return new Predicate<object>(o =>
            {
                string name = strfunc(o).RemoveWhitespace().ToUpperInvariant();
                bool contains = name.Contains(nameFilter);
                if (nameFilter.Length < name.Length)
                {
                    name = name.Substring(0, nameFilter.Length);
                }
                return contains || GenericHelpers.DamerauLevenshteinDistance(name, nameFilter, (nameFilter.Length + 1) / 2) != int.MaxValue;
            });
        }
    }
}
