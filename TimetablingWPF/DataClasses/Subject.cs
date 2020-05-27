using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TimetablingWPF;

namespace TimetablingWPF
{
    public class Subject : BaseDataClass
    {
        static Subject()
        {
            Type type = typeof(Subject);
            RegisterProperty(type, nameof(Groups));
            RegisterProperty(type, nameof(Teachers));
            RegisterProperty(type, nameof(Lessons));
        }
        public RelationalCollection<Group, Subject> Groups { get; private set; } = new RelationalCollection<Group, Subject>(nameof(Group.Subjects));
        public RelationalCollection<Teacher, Subject> Teachers { get; private set; } = new RelationalCollection<Teacher, Subject>(nameof(Teacher.Subjects));
        public InternalObservableCollection<Lesson> Lessons { get; private set; } = new InternalObservableCollection<Lesson>();
        private readonly IList<ErrorContainer> errorValidations = new List<ErrorContainer>()
        {

        };
        public override IEnumerable<ErrorContainer> ErrorValidations => errorValidations;

        public override void Save(BinaryWriter writer)
        {
            SaveParent(writer);
            Saving.WriteIntEnum(Teachers.Select(t => t.StorageIndex), writer);
        }

        public override void Load(BinaryReader reader, Version version, DataContainer container)
        {
            LoadParent(reader, version, container);
            Loading.LoadEnum(() => Teachers.Add(container.Teachers[reader.ReadInt32()]), reader);
        }
    }
}
