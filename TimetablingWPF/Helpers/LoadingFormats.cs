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
        public static int LATEST => versionMapping.Keys.Max();
        public delegate DataContainer LoadingDelegate(string fpath, BinaryReader reader, BackgroundWorker worker = null, DoWorkEventArgs e = null);
        private static readonly Dictionary<int, LoadingDelegate> versionMapping = new Dictionary<int, LoadingDelegate>()
        {
            {2, Version2}
        };
        public static LoadingDelegate GetLoadingDelegate(int version)
        {
            return versionMapping[version];
        }
        public static DataContainer Version2(string fpath, BinaryReader reader, BackgroundWorker worker = null, DoWorkEventArgs e = null)
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
            DataContainer data = new DataContainer();
            UpdateWorker("Timetable");
            data.SetTimetableStructure(
                LoadAndReturnList(() =>
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
                }, reader)
            );
            UpdateWorker("Year Groups");
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
            }, data, reader);
            UpdateWorker("Subjects");
            LoadBDCEnum<Subject>(s =>
            {
                LoadEnum(() => s.Teachers.Add(data.Teachers[reader.ReadInt32()]), reader);
            }, data, reader);
            UpdateWorker("Forms");
            LoadBDCEnum<Form>(f =>
            {
                f.YearGroup = data.YearGroups[reader.ReadInt32()];
            }, data, reader);
            UpdateWorker("Rooms");
            LoadBDCEnum<Room>(r =>
            {
                r.Quantity = reader.ReadInt32();
                r.Critical = reader.ReadBoolean();
            }, data, reader);
            UpdateWorker("Groups");
            LoadBDCEnum<Group>(g =>
            {
                LoadEnum(() => g.Subjects.Add(data.Subjects[reader.ReadInt32()]), reader);
                LoadEnum(() => g.Rooms.Add(data.Rooms[reader.ReadInt32()]), reader);
            }, data, reader);
            UpdateWorker("Lessons");
            LoadBDCEnum<Lesson>(l =>
            {
                LoadEnum(() => l.Forms.Add(data.Forms[reader.ReadInt32()]), reader);
                l.LessonsPerCycle = reader.ReadInt32();
                l.LessonLength = reader.ReadInt32();
                l.Subject = data.Subjects[reader.ReadInt32()];
                LoadEnum(() => l.Assignments.Add(new Assignment(data.Teachers[reader.ReadInt32()], l, reader.ReadInt32())), reader);
            }, data, reader);
            return data;
        }
    }
}
