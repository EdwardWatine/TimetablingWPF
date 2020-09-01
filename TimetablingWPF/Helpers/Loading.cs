using Humanizer;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimetablingWPF
{
    public static class Loading
    {
        public static void LoadEnum(Action action, BinaryReader reader)
        {
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                action();
            }
        }
        public static IList<T> LoadAndReturnList<T>(Func<T> func, BinaryReader reader)
        {
            int count = reader.ReadInt32();
            IList<T> retval = new List<T>(count);
            for (int i = 0; i < count; i++)
            {
                retval.Add(func());
            }
            return retval;
        }
        public static IList<int> LoadAndReturnIntList(BinaryReader reader)
        {
            return LoadAndReturnList(() => reader.ReadInt32(), reader);
        }
        public static void LoadBDCEnum<T>(BinaryReader reader, Version version, DataContainer container) where T : BaseDataClass, new()
        {
            LoadEnum(() =>
            {
                T obj = new T();
                obj.Load(reader, version, container);
                obj.Commit(container);
            }, reader);
        }
        public static DataContainer StartLoad(BinaryReader reader, Version version, BackgroundWorker worker = null, DoWorkEventArgs e = null)
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
            DataContainer container = new DataContainer();
            UpdateWorker("Timetable");
            TimetableStructure.Load(reader, version, container);
            UpdateWorker("Year Groups");
            LoadEnum(() =>
            {
                Year year = new Year();
                year.Load(reader, version, container);
                year.Commit(container);
            }, reader);
            object[] parameters = new object[] { reader, version, container };
            foreach (Type type in DataHelpers.UserTypes)
            {
                UpdateWorker(type.Name.Pluralize());
                typeof(Loading).GetMethod(nameof(LoadBDCEnum)).MakeGenericMethod(type).Invoke(null, parameters);
            }
            return container;
        }
    }
}
