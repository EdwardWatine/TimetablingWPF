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
        public static void WriteUshortEnum(IEnumerable<ushort> enumerable, BinaryWriter writer)
        {
            IList<ushort> list = enumerable.ToList();
            writer.Write((ushort)list.Count);
            for (ushort i = 0; i < list.Count; i++)
            {
                writer.Write(list[i]);
            }
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
            IList<Teacher> teacher_list = GetData<Teacher>();
            writer.Write((ushort)teacher_list.Count);
            for (ushort i = 0; i < teacher_list.Count; i++)
            {
                Teacher teacher = teacher_list[i];
                teacher.StorageIndex = i;
                writer.Write(teacher.Name);
                WriteUshortEnum(teacher.UnavailablePeriods.Select(p => p.ToUshort()), writer);
            }
            IList<Subject> subject_list = GetData<Subject>();
            writer.Write((ushort)subject_list.Count);
            for (ushort i = 0; i < subject_list.Count; i++)
            {
                Subject subject = subject_list[i];
                subject.StorageIndex = i;
                writer.Write(subject.Name);
                WriteUshortEnum(subject.Teachers.Select(t => t.StorageIndex), writer);
            }
            IList<Form> form_list = GetData<Form>();
            writer.Write((ushort)form_list.Count);
            for (ushort i = 0; i < form_list.Count; i++)
            {
                Form form = form_list[i];
                form.StorageIndex = i;
                writer.Write(form.Name);
                writer.Write(form.YearGroup.StorageIndex);
            }
            IList<Room> room_list = GetData<Room>();
            writer.Write((ushort)room_list.Count);
            for (ushort i = 0; i < form_list.Count; i++)
            {
                Room room = room_list[i];
                room.StorageIndex = i;
                writer.Write(room.Name);
                writer.Write(room.Quantity);
                writer.Write(room.Critical);
            }
            IList<Group> group_list = GetData<Group>();
            writer.Write((ushort)group_list.Count);
            for (ushort i = 0; i < group_list.Count; i++)
            {
                Group group = group_list[i];
                writer.Write(group.Name);
                WriteUshortEnum(group.Subjects.Select(s => s.StorageIndex), writer);
                WriteUshortEnum(group.Rooms.Select(r => r.StorageIndex), writer);
            }
            IList<Lesson> lesson_list = GetData<Lesson>();
            writer.Write((ushort)lesson_list.Count);
            for (ushort i = 0; i < lesson_list.Count; i++)
            {
                Lesson lesson = lesson_list[i];
                writer.Write(lesson.Name);
                writer.Write(lesson.Form.StorageIndex);
                writer.Write(lesson.LessonsPerCycle);
                writer.Write(lesson.LessonLength);
                writer.Write(lesson.Subject.StorageIndex);
                writer.Write(lesson.Assignments.Count);
                foreach (Assignment assignment in lesson.Assignments)
                {
                    writer.Write(assignment.Lesson.StorageIndex);
                    writer.Write(assignment.LessonCount);
                }
            }
            FileStream stream = File.OpenWrite(fpath);
            all_data.Seek(0, SeekOrigin.Begin);
            all_data.CopyTo(stream);
            stream.Close();
            writer.Close();
            all_data.Close();
        }
    }
}
