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
            RegisterProperty(type, nameof(Rooms), "Number of rooms");
            RegisterProperty(type, nameof(Groups));
            RegisterProperty(type, nameof(Teachers));
            RegisterProperty(type, nameof(Lessons));
        }
        public Subject()
        {
            Groups.AddToOther(RelatedGroup);
        }
        public RelationalCollection<Group, Subject> Groups { get; private set; } = new RelationalCollection<Group, Subject>(nameof(Group.Subjects));
        public RelationalCollection<Teacher, Subject> Teachers { get; private set; } = new RelationalCollection<Teacher, Subject>(nameof(Teacher.Subjects));
        public InternalObservableCollection<Lesson> Lessons { get; private set; } = new InternalObservableCollection<Lesson>();
        public Group RelatedGroup { get; private set; } = new Group();
        private int rooms;
        public int Rooms { get => rooms;
            set
            {
                if (rooms != value)
                {
                    rooms = value;
                    RelatedGroup.Rooms = value;
                    NotifyPropertyChanged(nameof(Rooms));
                }
            }
        }
        protected override void NotifyPropertyChanged(string prop)
        {
            if (prop == nameof(Name))
            {
                RelatedGroup.Name = Name;
            }
            if (prop == nameof(Shorthand))
            {
                RelatedGroup.Shorthand = Shorthand;
            }
            base.NotifyPropertyChanged(prop);
        }
        public override void Commit(DataContainer container = null)
        {
            if (!Committed)
            {
                RelatedGroup.Name = Name;
                RelatedGroup.Shorthand = Shorthand;
                RelatedGroup.Visible = false;
                RelatedGroup.Commit(container);
            }
            base.Commit(container);
        }
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
        public override void Delete(DataContainer container = null)
        {
            RelatedGroup.Delete();
            base.Delete(container);
        }
    }
}
