namespace TimetablingWPF
{
    public class Teacher : BaseDataClass
    {
        public ObservableCollection<TimetableSlot> UnavailablePeriods { get; private set; } = new ObservableCollection<TimetableSlot>();
        public RelationalCollection<Subject> Subjects { get; private set; } = new RelationalCollection<Subject>("Teachers");
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
        public RelationalCollection<Group> Groups { get; private set; } = new RelationalCollection<Group>("Subjects");
        public RelationalCollection<Teacher> Teachers { get; private set; } = new RelationalCollection<Teacher>("Subjects");
        public InternalObservableCollection<Lesson> Lessons { get; private set; } = new InternalObservableCollection<Lesson>();
    }

    public class Group : BaseDataClass
    {
        public RelationalCollection<Subject> Subjects { get; private set; } = new RelationalCollection<Subject>("Groups");
        public RelationalCollection<Room> Rooms { get; private set; } = new RelationalCollection<Room>("Groups");
    }
}
