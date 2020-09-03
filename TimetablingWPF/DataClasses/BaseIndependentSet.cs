using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ObservableComputations;
using static TimetablingWPF.GenericExtensions;

namespace TimetablingWPF
{
    public abstract class BaseIndependentSet : INotifyPropertyChanged, IDeleteable, ISaveable
    {
        public BaseIndependentSet(Year year)
        {
            Year = year;
            year.IndependentSets.Add(this);
        }
        public Year Year { get; }
        private string name = "<New Constraint>";
        public string Name
        {
            get => name;
            set
            {
                if (value != name)
                {
                    name = value;
                    RaisePropertyChanged(nameof(Name));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler Deleted;

        private bool valid = false;
        public bool ValidState
        {
            get => valid;
            protected set
            {
                if (value ^ valid)
                {
                    valid = value;
                    RaisePropertyChanged(nameof(ValidState));
                }
            }
        }
        protected virtual void RaisePropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public abstract IEnumerable<Form> GetUsefulForms(Form form); //'Useful' being independent and omitting guaranteed duplicate results
        public abstract BaseIndependentSet WithNewYear(Year year);

        public void Delete()
        {
            Year.IndependentSets.Remove(this);
            Deleted?.Invoke(this, EventArgs.Empty);
        }

        public void Save(BinaryWriter writer)
        {
            throw new NotImplementedException();
        }

        public void Load(BinaryReader reader, Version version, DataContainer container)
        {
            throw new NotImplementedException();
        }
    }
    public class ListIndependentSet : BaseIndependentSet
    {
        public ListIndependentSet(Year year) : base(year)
        {
            Forms = new ObservableCollectionExtended<Form>();
            ValidState = true;
        }
        public ListIndependentSet(Year year, Subject subject) : base(year)
        {
            if (subject == null)
            {
                Forms = new ObservableCollectionExtended<Form>();
            }
            else
            {
                Subject = subject;
                Forms = subject.Lessons.SelectingMany<Lesson, Form>(l => l.Forms).Distincting().Filtering(f => f.YearGroup == year);
                Name = subject.Name;
            }
            ValidState = true;
        }
        public override BaseIndependentSet WithNewYear(Year year)
        {
            ListIndependentSet lis = new ListIndependentSet(year, Subject);
            if (Subject == null)
            {
                lis.Forms = Forms;
            }
            return lis;
        }
        private object _prevent_gc_holder;
        private object _prevent_gc_holder_1;
        private ObservableCollectionWithChangeMarker<Form> forms;
        public ObservableCollectionWithChangeMarker<Form> Forms
        {
            get => forms;
            private set
            {
                forms = value;
                FormHashSet = value.Hashing(f => f);
                if (Subject == null)
                {
                    _prevent_gc_holder = Forms.LinkDeletions();
                    LinkFormYear();
                }
            }
        }
        private void FormYearChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Form.YearGroup))
            {
                Forms.Remove((Form)sender);
            }
        }
        private void LinkFormYear()
        {
            _prevent_gc_holder_1 = forms.ItemsProcessing<Form>(
                (f, ip, o, e) => f.PropertyChanged += FormYearChanged,
                (f, ip, o, e) => f.PropertyChanged -= FormYearChanged
            );
        }
        public Hashing<Form, Form> FormHashSet { get; private set; }
        public Subject Subject { get; }
        public override IEnumerable<Form> GetUsefulForms(Form form)
        {
            if (!FormHashSet.Contains(form))
            {
                return Enumerable.Empty<Form>();
            }
            return Forms.Except(form);
        }
    }
    public class HybridIndependentSet : BaseIndependentSet
    {
        public HybridIndependentSet(Year year) : base(year)
        {
            DependentHashSet = Dependents.Hashing(f => f);
            LinkFormYear();
            _prevent_gc_holder = Dependents.LinkDeletions();
            indepedent.BindPropertyChanged(() => RaisePropertyChanged(nameof(Independent)));
        }
        public ObservableCollectionExtended<Form> Dependents { get; private set; } = new ObservableCollectionExtended<Form>();
        public Hashing<Form, Form> DependentHashSet { get; private set; }
        private void FormYearChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Form.YearGroup))
            {
                Dependents.Remove((Form)sender);
            }
        }
        private void LinkFormYear()
        {
            _prevent_gc_holder_1 = Dependents.ItemsProcessing<Form>(
                (f, ip, o, e) => f.PropertyChanged += FormYearChanged,
                (f, ip, o, e) => f.PropertyChanged -= FormYearChanged
            );
        }
        object _prevent_gc_holder;
        object _prevent_gc_holder_1;
        private readonly DeleteableWrapper<ListIndependentSet> indepedent = new DeleteableWrapper<ListIndependentSet>();
        public ListIndependentSet Independent
        {
            get => indepedent.Value;
            set
            {
                indepedent.Value = value;
                ValidState = value != null;
            }
        }
        public override IEnumerable<Form> GetUsefulForms(Form form)
        {
            if (Independent.FormHashSet.Contains(form))
            {
                return Dependents.Except(form).Distinct();
            }
            if (DependentHashSet.Contains(form))
            {
                return Independent.Forms;
            }
            return Enumerable.Empty<Form>();
        }

        public override BaseIndependentSet WithNewYear(Year year)
        {
            HybridIndependentSet his = new HybridIndependentSet(year)
            {
                Independent = Independent,
                Dependents = Dependents,
                DependentHashSet = Dependents.Hashing(f => f)
            };
            return his;
        }
    }
}
