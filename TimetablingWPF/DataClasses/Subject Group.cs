namespace TimetablingWPF
{
    public class Subject : BaseDataClass
    {
        public RelationalCollection<Group, Subject> Groups { get; private set; } = new RelationalCollection<Group, Subject>("Subjects");
        public RelationalCollection<Teacher, Subject> Teachers { get; private set; } = new RelationalCollection<Teacher, Subject>("Subjects");
        public InternalObservableCollection<Lesson> Lessons { get; private set; } = new InternalObservableCollection<Lesson>();
    }

    public class Group : BaseDataClass
    {
        public RelationalCollection<Subject, Group> Subjects { get; private set; } = new RelationalCollection<Subject, Group>("Groups");
        public RelationalCollection<Room, Group> Rooms { get; private set; } = new RelationalCollection<Room, Group>("Groups");
    }
}
