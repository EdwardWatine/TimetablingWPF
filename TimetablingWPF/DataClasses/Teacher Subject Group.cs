namespace TimetablingWPF
{
    public class Teacher : BaseDataClass
    {
        public ObservableCollection<TimetableSlot> UnavailablePeriods { get; private set; } = new ObservableCollection<TimetableSlot>();
        public RelationalCollection<Subject> Subjects { get; private set; } = new RelationalCollection<Subject>("Teachers");
        public ObservableCollection<Assignment> Assignments { get; private set; } = new ObservableCollection<Assignment>();
        public const string ListName = "Teachers";
        public override string ListNameAbstract => ListName;
    }

    public class Subject : BaseDataClass
    {
        public RelationalCollection<Group> Groups { get; private set; } = new RelationalCollection<Group>("Subjects");
        public RelationalCollection<Teacher> Teachers { get; private set; } = new RelationalCollection<Teacher>("Subjects");
        public const string ListName = "Subjects";
        public override string ListNameAbstract => ListName;

    }

    public class Group : BaseDataClass
    {
        public RelationalCollection<Band> Classes { get; private set; } = new RelationalCollection<Band>("Groups");
        public RelationalCollection<Subject> Subjects { get; private set; } = new RelationalCollection<Subject>("Groups");
        public const string ListName = "Groups";
        public override string ListNameAbstract => ListName;
    }
}
