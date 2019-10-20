namespace TimetablingWPF
{
    public class Teacher : BaseDataClass
    {
        public ObservableCollection<TimetableSlot> UnavailablePeriods { get; private set; } = new ObservableCollection<TimetableSlot>();
        public RelationalList<Subject> Subjects { get; private set; } = new RelationalList<Subject>("Teachers");
        public ObservableCollection<Assignment> Assignments { get; private set; } = new ObservableCollection<Assignment>();
        public const string ListName = "Teachers";
        public override string ListNameAbstract => ListName;
    }

    public class Subject : BaseDataClass
    {
        public RelationalList<Group> Groups { get; private set; } = new RelationalList<Group>("Subjects");
        public RelationalList<Teacher> Teachers { get; private set; } = new RelationalList<Teacher>("Subjects");
        public const string ListName = "Subjects";
        public override string ListNameAbstract => ListName;

    }

    public class Group : BaseDataClass
    {
        public RelationalList<Class> Classes { get; private set; } = new RelationalList<Class>("Groups");
        public RelationalList<Subject> Subjects { get; private set; } = new RelationalList<Subject>("Groups");
        public const string ListName = "Groups";
        public override string ListNameAbstract => ListName;
    }
}
