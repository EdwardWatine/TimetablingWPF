using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TimetablingWPF.FileHelpers;
using static TimetablingWPF.DataHelpers;

namespace TimetablingWPF
{
    public static class LoadingFormats
    {
        public const int LATEST = 2;
        public delegate void LoadingDelegate(string fpath, BinaryReader reader, BackgroundWorker worker = null, DoWorkEventArgs e = null);
        private static readonly Dictionary<int, LoadingDelegate> versionMapping = new Dictionary<int, LoadingDelegate>()
        {
            {1, Version1},
            {2, Version2}
        };
        public static LoadingDelegate GetLoadingDelegate(int version)
        {
            return versionMapping[version];
        }
        public static void Version1(string fpath, BinaryReader reader, BackgroundWorker worker = null, DoWorkEventArgs e = null)
        {
            int total = 7;
            int count = 0;
            void UpdateWorker(string things)
            {
                worker?.ReportProgress(count * 100 / 7, $"Loading {things} ({count + 1}/{total})");
                count++;
                if (worker?.CancellationPending ?? false)
                {
                    e.Cancel = true;
                }
            };
            UpdateWorker("Lessons");
            LoadEnum(() =>
            {
                string name = reader.ReadString();
                YearGroup year = new YearGroup(name);
                year.Commit();
            }, reader);
            UpdateWorker("Teachers");
            LoadBDCEnum<Teacher>(t =>
            {
                t.MaxPeriodsPerCycle = reader.ReadInt32();
                LoadEnum(() => t.UnavailablePeriods.Add(TimetableSlot.FromInt(reader.ReadInt32())), reader);
            }, reader);
            UpdateWorker("Subjects");
            IList<Teacher> teacher_list = GetData<Teacher>();
            LoadBDCEnum<Subject>(s =>
            {
                LoadEnum(() => s.Teachers.Add(teacher_list[reader.ReadInt32()]), reader);
            }, reader);
            UpdateWorker("Forms");
            IList<YearGroup> year_list = GetData<YearGroup>();
            LoadBDCEnum<Form>(f =>
            {
                f.YearGroup = year_list[reader.ReadInt32()];
            }, reader);
            UpdateWorker("Rooms");
            LoadBDCEnum<Room>(r =>
            {
                r.Quantity = reader.ReadInt32();
                r.Critical = reader.ReadBoolean();
            }, reader);
            UpdateWorker("Groups");
            IList<Subject> subject_list = GetData<Subject>();
            IList<Room> room_list = GetData<Room>();
            LoadBDCEnum<Group>(g =>
            {
                LoadEnum(() => g.Subjects.Add(subject_list[reader.ReadInt32()]), reader);
                LoadEnum(() => g.Rooms.Add(room_list[reader.ReadInt32()]), reader);
            }, reader);
            UpdateWorker("Lessons");
            IList<Form> form_list = GetData<Form>();
            LoadBDCEnum<Lesson>(l =>
            {
                LoadEnum(() => l.Forms.Add(form_list[reader.ReadInt32()]), reader);
                l.LessonsPerCycle = reader.ReadInt32();
                l.LessonLength = reader.ReadInt32();
                l.Subject = subject_list[reader.ReadInt32()];
                LoadEnum(() => l.Assignments.Add(new Assignment(teacher_list[reader.ReadInt32()], l, reader.ReadInt32())), reader);
            }, reader);
        }
        public static void Version2(string fpath, BinaryReader reader, BackgroundWorker worker = null, DoWorkEventArgs e = null)
        {
            int total = 8;
            int count = 0;
            void UpdateWorker(string things)
            {
                worker?.ReportProgress(count * 100 / total, $"Loading {things} ({count + 1}/{total})");
                count++;
                if (worker?.CancellationPending ?? false)
                {
                    e.Cancel = true;
                }
            };
            UpdateWorker("Timetable");
            TimetableStructure.SetData(LoadAndReturnList(() =>
            {
                return new StructureClasses.TimetableStructureWeek(reader.ReadString(),
                    LoadAndReturnList(() =>
                    {
                        return reader.ReadString();
                    }, reader),
                    LoadAndReturnList(() =>
                    {
                        return reader.ReadString();
                    }, reader),
                    LoadAndReturnIntList(reader)
                    );
            }, reader));
            UpdateWorker("Lessons");
            LoadEnum(() =>
            {
                string name = reader.ReadString();
                YearGroup year = new YearGroup(name);
                year.Commit();
            }, reader);
            UpdateWorker("Teachers");
            LoadBDCEnum<Teacher>(t =>
            {
                t.MaxPeriodsPerCycle = reader.ReadInt32();
                LoadEnum(() => t.UnavailablePeriods.Add(TimetableSlot.FromInt(reader.ReadInt32())), reader);
            }, reader);
            UpdateWorker("Subjects");
            IList<Teacher> teacher_list = GetData<Teacher>();
            LoadBDCEnum<Subject>(s =>
            {
                LoadEnum(() => s.Teachers.Add(teacher_list[reader.ReadInt32()]), reader);
            }, reader);
            UpdateWorker("Forms");
            IList<YearGroup> year_list = GetData<YearGroup>();
            LoadBDCEnum<Form>(f =>
            {
                f.YearGroup = year_list[reader.ReadInt32()];
            }, reader);
            UpdateWorker("Rooms");
            LoadBDCEnum<Room>(r =>
            {
                r.Quantity = reader.ReadInt32();
                r.Critical = reader.ReadBoolean();
            }, reader);
            UpdateWorker("Groups");
            IList<Subject> subject_list = GetData<Subject>();
            IList<Room> room_list = GetData<Room>();
            LoadBDCEnum<Group>(g =>
            {
                LoadEnum(() => g.Subjects.Add(subject_list[reader.ReadInt32()]), reader);
                LoadEnum(() => g.Rooms.Add(room_list[reader.ReadInt32()]), reader);
            }, reader);
            UpdateWorker("Lessons");
            IList<Form> form_list = GetData<Form>();
            LoadBDCEnum<Lesson>(l =>
            {
                LoadEnum(() => l.Forms.Add(form_list[reader.ReadInt32()]), reader);
                l.LessonsPerCycle = reader.ReadInt32();
                l.LessonLength = reader.ReadInt32();
                l.Subject = subject_list[reader.ReadInt32()];
                LoadEnum(() => l.Assignments.Add(new Assignment(teacher_list[reader.ReadInt32()], l, reader.ReadInt32())), reader);
            }, reader);
        }
    }
}
