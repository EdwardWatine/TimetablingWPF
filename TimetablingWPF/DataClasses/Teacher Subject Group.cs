namespace TimetablingWPF
{
    public class Teacher : BaseDataClass
    {
        public ObservableCollection<TimetableSlot> UnavailablePeriods { get; private set; } = new ObservableCollection<TimetableSlot>();
        public RelationalCollection<Subject> Subjects { get; private set; } = new RelationalCollection<Subject>("Teachers");
        public ObservableCollection<Assignment> Assignments { get; private set; } = new ObservableCollection<Assignment>();
        public const string ListName = "Teachers";
        protected override string ListNameAbstract => ListName;
    }

    public class Subject : BaseDataClass
    {
        public RelationalCollection<Group> Groups { get; private set; } = new RelationalCollection<Group>("Subjects");
        public RelationalCollection<Teacher> Teachers { get; private set; } = new RelationalCollection<Teacher>("Subjects");
        public const string ListName = "Subjects";
        protected override string ListNameAbstract => ListName;

    }

    public class Group : BaseDataClass
    {
        public RelationalCollection<Form> Setes { get; private set; } = new RelationalCollection<Form>("Groups");
        public RelationalCollection<Subject> Subjects { get; private set; } = new RelationalCollection<Subject>("Groups");
        public RelationalCollection<Room> Rooms { get; private set; } = new RelationalCollection<Room>("Groups");
        public const string ListName = "Groups";
        protected override string ListNameAbstract => ListName;
    }
}
