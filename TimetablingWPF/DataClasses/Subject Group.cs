using System;
using System.Collections.Generic;

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
    }

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
    }
}
