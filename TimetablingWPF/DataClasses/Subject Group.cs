using System;

namespace TimetablingWPF
{
    public class Subject : BaseDataClass
    {
        static Subject()
        {
            Type type = typeof(Subject);
            RegisterProperty(type, type.GetProperty("Groups"));
            RegisterProperty(type, type.GetProperty("Teachers"));
            RegisterProperty(type, type.GetProperty("Lessons"));
        }
        public RelationalCollection<Group, Subject> Groups { get; private set; } = new RelationalCollection<Group, Subject>("Subjects");
        public RelationalCollection<Teacher, Subject> Teachers { get; private set; } = new RelationalCollection<Teacher, Subject>("Subjects");
        public InternalObservableCollection<Lesson> Lessons { get; private set; } = new InternalObservableCollection<Lesson>();
    }

    public class Group : BaseDataClass
    {
        static Group()
        {
            Type type = typeof(Group);
            RegisterProperty(type, type.GetProperty("Subjects"));
            RegisterProperty(type, type.GetProperty("Rooms"));
        }
        public RelationalCollection<Subject, Group> Subjects { get; private set; } = new RelationalCollection<Subject, Group>("Groups");
        public RelationalCollection<Room, Group> Rooms { get; private set; } = new RelationalCollection<Room, Group>("Groups");
    }
}
