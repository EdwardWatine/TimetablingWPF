using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static TimetablingWPF.DataHelpers;

namespace TimetablingWPF
{
    public static class FileHandlers
    {
        private static void WriteUshortEnum(IEnumerable<int> enumerable, BinaryWriter writer)
        {
            IList<int> list = enumerable.ToList();
            writer.Write(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                writer.Write(list[i]);
            }
        }
        private static void WriteBDCEnum<T>(IEnumerable<T> enumerable, BinaryWriter writer, Action<T> action) where T : BaseDataClass
        {
            IList<T> list = enumerable.ToList();
            writer.Write(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                T obj = list[i];
                obj.StorageIndex = i;
                writer.Write(obj.Name);
                action(obj);
            }
        }
        private static void LoadEnum(Action action, BinaryReader reader)
        {
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                action();
            }
        }
        private static void LoadBDCEnum<T>(Action<T> action, BinaryReader reader) where T : BaseDataClass, new()
        {
            LoadEnum(() =>
            {
                T obj = new T() { Name = reader.ReadString() };
                action(obj);
                obj.Commit();
            }, reader);
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
            LoadBDCEnum<Teacher>(t =>
            {
                LoadEnum(() => t.UnavailablePeriods.Add(TimetableSlot.FromInt(reader.ReadInt32())), reader);
            }, reader);
            IList<Teacher> teacher_list = GetData<Teacher>();
            LoadBDCEnum<Subject>(s =>
            {
                LoadEnum(() => s.Teachers.Add(teacher_list[reader.ReadInt32()]), reader);
            }, reader);
            IList<YearGroup> year_list = GetData<YearGroup>();
            LoadBDCEnum<Form>(f =>
            {
                f.YearGroup = year_list[reader.ReadInt32()];
            }, reader);
            LoadBDCEnum<Room>(r =>
            {
                r.Quantity = reader.ReadInt32();
                r.Critical = reader.ReadBoolean();
            }, reader);
            IList<Subject> subject_list = GetData<Subject>();
            IList<Room> room_list = GetData<Room>();
            LoadBDCEnum<Group>(g =>
            {
                LoadEnum(() => g.Subjects.Add(subject_list[reader.ReadInt32()]), reader);
                LoadEnum(() => g.Rooms.Add(room_list[reader.ReadInt32()]), reader);
            }, reader);
            IList<Form> form_list = GetData<Form>();
            LoadBDCEnum<Lesson>(l =>
            {
                l.Form = form_list[reader.ReadInt32()];
                l.LessonsPerCycle = reader.ReadInt32();
                l.LessonLength = reader.ReadInt32();
                l.Subject = subject_list[reader.ReadInt32()];
                LoadEnum(() => l.Assignments.Add(new Assignment(teacher_list[reader.ReadInt32()], l, reader.ReadInt32())), reader);
            }, reader);
            reader.Close();
            fileStream.Close();
        }
        public static void SaveData(string fpath)
        {
            MemoryStream all_data = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(all_data);
            IList<YearGroup> year_list = GetData<YearGroup>();
            writer.Write((int)year_list.Count);
            for (int i = 0; i < year_list.Count; i++)
            {
                YearGroup year = year_list[i];
                year.StorageIndex = i;
                writer.Write(year.Year);
            }
            WriteBDCEnum(GetData<Teacher>(), writer, t =>
            {
                WriteUshortEnum(t.UnavailablePeriods.Select(p => p.ToInt()), writer);
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
