using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using ObservableComputations;

namespace TimetablingWPF
{
    public class Year : IDeleteable, INotifyPropertyChanged, ISaveable
    {
        public Year() { }
        public Year(string name)
        {
            Name = name;
        }
        public bool Committed { get; private set; } = false;
        public HashSet<BaseIndependentSet> IndependentSets { get; } = new HashSet<BaseIndependentSet>();
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler Deleted;

        public void NotifyPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public ObservableCollectionExtended<Form> Forms { get; private set; } = new ObservableCollectionExtended<Form>();
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
                (container ?? App.Data).YearGroups.Add(this);
                Committed = true;
            }
        }
        public void Delete(DataContainer container = null)
        {
            foreach (Form form in Forms)
            {
                form.YearGroup = App.Data.NoneYear;
            }
            (container ?? App.Data).YearGroups.Remove(this);
            Deleted?.Invoke(this, EventArgs.Empty);
        }
        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public void Delete()
        {
            Delete(null);
        }

        public virtual void Save(BinaryWriter writer)
        {
            writer.Write(Name);
        }

        public void Load(BinaryReader reader, Version version, DataContainer container)
        {
            Name = reader.ReadString();
        }

        public static bool operator ==(Year left, Year right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Year left, Year right)
        {
            return !(left == right);
        }
    }
}
