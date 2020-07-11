using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace TimetablingWPF
{
    public static class Saving
    {
        public static void WriteIntEnum(IEnumerable<int> enumerable, BinaryWriter writer)
        {
            WriteList(enumerable.ToList(), (i, index) => writer.Write(i), writer);
        }
        public static void WriteBDCList<T>(IList<T> list, BinaryWriter writer) where T : BaseDataClass
        {
            writer.Write(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                T obj = list[i];
                obj.StorageIndex = i;
                obj.Save(writer);
            }
        }

        public static void WriteList<T>(IList<T> list, Action<T, int> action, BinaryWriter writer)
        {
            writer.Write(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                action(list[i], i);
            }
        }

        public static void StartSave(BinaryWriter writer, DataContainer data)
        {
            Thread.Sleep(5000);
            writer.Write(Assembly.GetExecutingAssembly().GetName().Version.ToString());
            TimetableStructure.Save(writer);
            WriteList(data.YearGroups, (y, i) =>
            {
                y.StorageIndex = i;
                writer.Write(y.Name);
                writer.Write(y.Visible);
            }, writer);
            foreach (Type type in DataHelpers.UserTypes)
            {
                typeof(Saving).GetMethod(nameof(WriteBDCList)).MakeGenericMethod(type).Invoke(null, new object[] { data.FromType(type), writer });
            }
        }
    }
}
