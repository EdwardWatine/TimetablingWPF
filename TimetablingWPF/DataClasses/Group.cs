using System;
using System.Collections.Generic;
using System.IO;
using TimetablingWPF;
using System.Linq;

namespace TimetablingWPF
{
    public class Group : BaseDataClass
    {
        static Group()
        {
            Type type = typeof(Group);
            RegisterProperty(type, nameof(Subjects));
            RegisterProperty(type, nameof(Rooms));
        }
        public RelationalCollection<Subject, Group> Subjects { get; private set; } = new RelationalCollection<Subject, Group>(nameof(Subject.Groups));
        public RelationalCollection<Room, Group> Rooms { get; private set; } = new RelationalCollection<Room, Group>(nameof(Room.Groups));
        private readonly IList<ErrorContainer> errorValidations = new List<ErrorContainer>()
        {

        };
        public override IEnumerable<ErrorContainer> ErrorValidations => errorValidations;

        public override void Save(BinaryWriter writer)
        {
            SaveParent(writer);
            Saving.WriteIntEnum(Subjects.Select(s => s.StorageIndex), writer);
            Saving.WriteIntEnum(Rooms.Select(r => r.StorageIndex), writer);
        }

        public override void Load(BinaryReader reader, Version version, DataContainer container)
        {
            LoadParent(reader, version, container);
            Loading.LoadEnum(() => Subjects.Add(container.Subjects[reader.ReadInt32()]), reader);
            Loading.LoadEnum(() => Rooms.Add(container.Rooms[reader.ReadInt32()]), reader);
        }
    }
}
