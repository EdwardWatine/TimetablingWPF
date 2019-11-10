namespace TimetablingWPF
{
    public class Teacher : BaseDataClass
    {
        public ObservableCollection<TimetableSlot> UnavailablePeriods { get; private set; } = new ObservableCollection<TimetableSlot>();
        public RelationalCollection<Subject, Teacher> Subjects { get; private set; } = new RelationalCollection<Subject, Teacher>("Teachers");
        public ObservableCollection<Assignment> Assignments { get; private set; } = new ObservableCollection<Assignment>();
        public override void Commit()
        {
            foreach (Assignment assignment in Assignments)
            {
                assignment.Lesson.Assignments.Add(assignment);
            }
            base.Commit();
        }
    }

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
