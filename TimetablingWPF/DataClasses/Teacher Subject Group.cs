namespace TimetablingWPF
{
    public class Teacher : BaseDataClass
    {
        public ObservableCollection<TimetableSlot> UnavailablePeriods { get; private set; } = new ObservableCollection<TimetableSlot>();
        public RelationalList<Subject> Subjects { get; private set; } = new RelationalList<Subject>("Teachers");
        public ObservableCollection<Assignment> Assignments { get; private set; } = new ObservableCollection<Assignment>();
        public const string ListName = "Teachers";
        protected override string ListNameAbstract => ListName;
    }

    public class Subject : BaseDataClass
    {
        public RelationalList<Room> Rooms { get; private set; } = new RelationalList<Room>("Subjects");
        public RelationalList<Teacher> Teachers { get; private set; } = new RelationalList<Teacher>("Subjects");
        public const string ListName = "Subjects";
        protected override string ListNameAbstract => ListName;

    }

    public class Group : BaseDataClass
    {
        public RelationalList<Class> Classes { get; private set; } = new RelationalList<Class>("Groups");
        public const string ListName = "Groups";
        protected override string ListNameAbstract => ListName;
    }
}
