using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static TimetablingWPF.DataHelpers;
using static TimetablingWPF.GenericHelpers;

namespace TimetablingWPF
{
    public static class FileHandlers
    {
        private static void WriteUshortEnum(IEnumerable<ushort> enumerable, BinaryWriter writer)
        {
            IList<ushort> list = enumerable.ToList();
            writer.Write((ushort)list.Count);
            for (ushort i = 0; i < list.Count; i++)
            {
                writer.Write(list[i]);
            }
        }
        private static void WriteBDCEnum<T>(IEnumerable<T> enumerable, BinaryWriter writer, Action<T> action) where T : BaseDataClass
        {
            IList<T> list = enumerable.ToList();
            writer.Write((ushort)list.Count);
            for (ushort i = 0; i < list.Count; i++)
            {
                T obj = list[i];
                obj.StorageIndex = i;
                writer.Write(obj.Name);
                action(obj);
            }
        }
        public static void LoadEnum(Action action, BinaryReader reader)
        {
            ushort count = reader.ReadUInt16();
            for (ushort i = 0; i < count; i++)
            {
                action();
            }
        }
        public static IEnumerable<T> LoadEnum<T>(Func<T> func, BinaryReader reader)
        {
            ushort count = reader.ReadUInt16();
            for (ushort i = 0; i < count; i++)
            {
                yield return func();
            }
        }
        public static void LoadData(string fpath)
        {
            FileStream fileStream = File.OpenRead(fpath);
            BinaryReader reader = new BinaryReader(fileStream);
            LoadEnum(() =>
            {
                string name = reader.ReadString();
                YearGroup year = new YearGroup(name);
                year.Commit();
            }, reader);
            LoadEnum(() =>
            {
                Teacher teacher = new Teacher() { Name = reader.ReadString() };
                teacher.UnavailablePeriods.AddRange(LoadEnum<TimetableSlot>(() =>
                {
                    ushort unavailable_num = reader.ReadUInt16();
                    return TimetableSlot.FromInt(unavailable_num);
                }, reader));
            }, reader);
        }
        public static void SaveData(string fpath)
        {
            MemoryStream all_data = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(all_data);
            IList<YearGroup> year_list = GetData<YearGroup>();
            writer.Write((ushort)year_list.Count);
            for (ushort i = 0; i < year_list.Count; i++)
            {
                YearGroup year = year_list[i];
                year.StorageIndex = i;
                writer.Write(year.Year);
            }
            WriteBDCEnum(GetData<Teacher>(), writer, t =>
            {
                WriteUshortEnum(t.UnavailablePeriods.Select(p => p.ToUshort()), writer);
            });
            WriteBDCEnum(GetData<Subject>(), writer, s =>
            {
                WriteUshortEnum(s.Teachers.Select(t => t.StorageIndex), writer);
            });
            WriteBDCEnum(GetData<Form>(), writer, f =>
            {
                writer.Write(f.YearGroup.StorageIndex);
            });
            WriteBDCEnum(GetData<Room>(), writer, r =>
            {
                writer.Write(r.Quantity);
                writer.Write(r.Critical);
            });
            WriteBDCEnum(GetData<Group>(), writer, g =>
            {
                WriteUshortEnum(g.Subjects.Select(s => s.StorageIndex), writer);
                WriteUshortEnum(g.Rooms.Select(r => r.StorageIndex), writer);
            });
            WriteBDCEnum(GetData<Lesson>(), writer, l =>
            {
                writer.Write(l.Name);
                writer.Write(l.Form.StorageIndex);
                writer.Write(l.LessonsPerCycle);
                writer.Write(l.LessonLength);
                writer.Write(l.Subject.StorageIndex);
                writer.Write(l.Assignments.Count);
                foreach (Assignment assignment in l.Assignments)
                {
                    writer.Write(assignment.Teacher.StorageIndex);
                    writer.Write(assignment.LessonCount);
                }
            });
            all_data.Seek(0, SeekOrigin.Begin);
            FileStream stream = File.OpenWrite(fpath);
            all_data.CopyTo(stream);
            stream.Close();
            writer.Close();
            all_data.Close();
        }
    }
}
