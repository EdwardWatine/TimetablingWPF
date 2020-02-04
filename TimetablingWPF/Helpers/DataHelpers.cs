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
            return (TabItem)Activator.CreateInstance(TabMappings[item.GetType()], new object[] { item, commandType });
        }
        public static readonly Dictionary<Type, Type> TabMappings = new Dictionary<Type, Type>()
        {
            {typeof(Subject), typeof(SubjectTab) },
            {typeof(Teacher), typeof(TeacherTab) },
            {typeof(Lesson), typeof(LessonTab) },
            {typeof(Group), typeof(GroupTab) },
            {typeof(Form), typeof(FormTab) },
            {typeof(Room), typeof(RoomTab) }

        };
    }
}
