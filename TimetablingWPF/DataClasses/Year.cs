using System.ComponentModel;

namespace TimetablingWPF
{
    public class Year : INotifyPropertyChanged
    {
        public Year(string name)
        {
            Name = name;
        }
        public bool Committed { get; private set; } = false;

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public InternalObservableCollection<Form> Forms { get; private set; } = new InternalObservableCollection<Form>();
        private string _year;
        public string Name
        {
            get => _year; set
            {
                if (value != _year)
                {
                    NotifyPropertyChanged(nameof(Year));
                    _year = value;
                }
            }
        }
        public int StorageIndex { get; set; }
        public void Commit(DataContainer container = null)
        {
            if (!Committed)
            {
                (container ?? DataHelpers.GetDataContainer()).YearGroups.Add(this);
                Committed = true;
            }
        }
        public void Delete(DataContainer container = null)
        {
            foreach (Form form in Forms)
            {
                form.YearGroup = null;
            }
            (container ?? DataHelpers.GetDataContainer()).YearGroups.Remove(this);
        }
        public override string ToString()
        {
            return $"Year {Name}";
        }

        public override bool Equals(object obj)
        {
            return obj is Year yg && yg.Name == Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
        public static bool operator ==(Year left, Year right)
        {
            return ReferenceEquals(left, right);
        }

        public static bool operator !=(Year left, Year right)
        {
            return !(left == right);
        }
    }
}
