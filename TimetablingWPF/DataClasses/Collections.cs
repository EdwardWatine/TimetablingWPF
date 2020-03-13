using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimetablingWPF
{
    //Provides some extra functionality on top of the existing observable collection
    public class ObservableCollection<T> : System.Collections.ObjectModel.ObservableCollection<T>, ICloneable, IAddRange
    {
        protected bool SuppressEvent { get; set; } = false;
        public ObservableCollection() { }
        public ObservableCollection(IEnumerable<T> collection) : base(collection) { }
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!SuppressEvent) // prevent the list from propagating changes, mostly to prevent functions like AddRange propagating multiple events
            {
                base.OnCollectionChanged(e);
            }
        }
        public void AddRange(IEnumerable<T> enumerable)
        {
            bool flag = SuppressEvent;
            SuppressEvent = true;
            foreach (T item in enumerable)
            {
                Add(item);
            }
            SuppressEvent = flag || false; // ensures that SuppressEvent is not accidently overwritten
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, enumerable.ToList()));
        }
        public void SetData(IEnumerable<T> enumerable)
        {
            Clear();
            AddRange(enumerable);
        }
        public virtual object Clone()
        {
            return new ObservableCollection<T>(this);
        }

        public void AddRange(IEnumerable<object> enumerable)
        {
            AddRange(enumerable.Cast<T>());
        }
    }
    /// <remarks>
    /// Implementing item property changes taken from https://stackoverflow.com/a/5256827
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public class InternalObservableCollection<T> : ObservableCollection<T> where T : INotifyPropertyChanged
    {
        public InternalObservableCollection()
        {
            CollectionChanged += ObservableCollectionCollectionChanged;
        }
        public InternalObservableCollection(IEnumerable<T> collection) : base(collection)
        {
            CollectionChanged += ObservableCollectionCollectionChanged;
        }
        private void ObservableCollectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (object item in e.NewItems)
                {
                    ((INotifyPropertyChanged)item).PropertyChanged += ItemPropertyChanged; // link event handler
                }
            }
            if (e.OldItems != null)
            {
                foreach (object item in e.OldItems)
                {
                    ((INotifyPropertyChanged)item).PropertyChanged -= ItemPropertyChanged; // unlink event handler
                }
            }
        }
        private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            int index = IndexOf((T)sender);
            NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, sender, sender, index);
            OnCollectionChanged(args); // propagate CollectionChanged when PropertyChanged is raised
        }
        public override object Clone()
        {
            return new InternalObservableCollection<T>(this);
        }
    }
    /// <summary>
    /// A list which reflects updates in itself with the list in the added class
    /// </summary>
    /// <typeparam name="TContent">The type of the objects in this list</typeparam>
    /// <typeparam name="TThis">The type of the object declaring this list</typeparam>
    public class RelationalCollection<TContent, TThis> : InternalObservableCollection<TContent>, IRelationalCollection, IFreezable
        where TContent : INotifyPropertyChanged
        where TThis : INotifyPropertyChanged
    {
        /// <summary>
        /// The object to which this list belongs
        /// </summary>
        public object Parent
        {
            get => _parent;
            set => _parent = (TThis)value;
        }
        private TThis _parent;
        /// <summary>
        /// Holds the field name of the object of type T which holds the RelationalList which will hold the parent
        /// </summary>
        public string OtherSetProperty { get; set; }
        /// <summary>
        /// The form constructor
        /// </summary>
        /// <param name="otherSetProperty"><see cref="OtherSetProperty"/></param>
        public RelationalCollection(string otherSetProperty)
        {
            OtherSetProperty = otherSetProperty;
        }

        public RelationalCollection(string otherSetProperty,
            IEnumerable<TContent> collection) : base(collection)
        {
            OtherSetProperty = otherSetProperty;
        }
        /// <summary>
        /// Adds a new item to the list, and adds the parent in the list of the added item
        /// </summary>
        /// <param name="item">Item to add to the list</param>
        protected override void InsertItem(int index, TContent item)
        {
            if (_parent == null)
            {
                throw new InvalidOperationException("Parent is not set");
            }
            base.InsertItem(index, item);
            if (Frozen)
            {
                frozenAddElements.Enqueue(item);
                return;
            }
            AddToOther(item);
        }
        protected virtual void OnlyInsert(int index, TContent item)
        {
            if (_parent == null)
            {
                throw new InvalidOperationException("Parent is not set");
            }
            base.InsertItem(index, item);
        }
        private void AddToOther(TContent item)
        {
            RelationalCollection<TThis, TContent> target = (RelationalCollection<TThis, TContent>)typeof(TContent).GetProperty(OtherSetProperty).GetValue(item);
            target.OnlyInsert(target.Count, _parent); // add the item to the corresponding RelationalCollection
        }
        protected override void RemoveItem(int index)
        {
            if (Parent == null)
            {
                throw new InvalidOperationException("Parent is not set");
            }
            base.RemoveItem(index);
            TContent item = this[index];
            if (Frozen)
            {
                frozenRemoveElements.Enqueue(item);
                return;
            }
            RemoveFromOther(item);
        }
        protected override void ClearItems()
        {
            foreach (TContent obj in this)
            {
                RemoveFromOther(obj); // removes the parent from all of its RelationalLists
            }
            base.ClearItems();
        }
        protected override void SetItem(int index, TContent item)
        {
            RemoveFromOther(this[index]);
            base.SetItem(index, item);
            AddToOther(item);
        }
        protected virtual void OnlyRemove(int index)
        {
            base.RemoveItem(index);
        }
        private void RemoveFromOther(TContent item)
        {
            RelationalCollection<TThis, TContent> target = (RelationalCollection<TThis, TContent>)typeof(TContent).GetProperty(OtherSetProperty).GetValue(item);
            target.OnlyRemove(target.IndexOf(_parent)); // remove the item to the corresponding RelationalCollection
        }
        public void Freeze()
        {
            Frozen = true;
        }
        public void Unfreeze()
        {
            Frozen = false;
            while (frozenAddElements.Count > 0)
            {
                AddToOther(frozenAddElements.Dequeue()); // unfreeze and add the elements
            }
            while (frozenRemoveElements.Count > 0)
            {
                RemoveFromOther(frozenRemoveElements.Dequeue()); // unfreeze and remove the elements
            }
        }
        public override object Clone()
        {
            return new RelationalCollection<TContent, TThis>(OtherSetProperty, this) { Parent = Parent }; // duplicate the RelationalList
        }
        public bool Frozen { get; private set; } = false;
        private readonly Queue<TContent> frozenAddElements = new Queue<TContent>();
        private readonly Queue<TContent> frozenRemoveElements = new Queue<TContent>();
    }
}
