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
    public class ObservableCollection<T> : System.Collections.ObjectModel.ObservableCollection<T>, ICloneable
    {
        public ObservableCollection() { }
        public ObservableCollection(IEnumerable<T> collection) : base(collection) { }
        public void AddRange(IEnumerable<T> enumerable)
        {
            foreach (T item in enumerable)
            {
                Add(item);
            }
        }
        public virtual object Clone()
        {
            return new ObservableCollection<T>(this);
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
                    ((INotifyPropertyChanged)item).PropertyChanged += ItemPropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (object item in e.OldItems)
                {
                    ((INotifyPropertyChanged)item).PropertyChanged -= ItemPropertyChanged;
                }
            }
        }
        private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            int index = IndexOf((T)sender);
            NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, sender, sender, index);
            OnCollectionChanged(args);
        }
        public override object Clone()
        {
            return new InternalObservableCollection<T>(this);
        }
    }
    /// <summary>
    /// A list which reflects updates in itself with the list in the added form
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
        public new void Add(TContent item)
        {
            if (_parent == null)
            {
                throw new InvalidOperationException("Parent is not set");
            }
            base.Add(item);
            if (Frozen)
            {
                frozenAddElements.Add(item);
                return;
            }
            AddToOther(item);
        }
        private void AddToOther(TContent item)
        {
            ((ObservableCollection<TThis>)typeof(TContent).GetProperty(OtherSetProperty).GetValue(item)).Add(_parent);
        }
        public new void Remove(TContent item)
        {
            if (Parent == null)
            {
                throw new InvalidOperationException("Parent is not set");
            }
            base.Remove(item);
            if (Frozen)
            {
                frozenRemoveElements.Add(item);
                return;
            }
            RemoveFromOther(item);
        }
        private void RemoveFromOther(TContent item)
        {
            ((ObservableCollection<TThis>)typeof(TContent).GetProperty(OtherSetProperty).GetValue(item)).Remove(_parent);
        }
        public void Freeze()
        {
            Frozen = true;
        }
        public new void AddRange(IEnumerable<TContent> enumerable)
        {
            foreach (TContent item in enumerable)
            {
                Add(item);
            }
        }
        public void Unfreeze()
        {
            Frozen = false;
            foreach (TContent element in frozenRemoveElements)
            {
                RemoveFromOther(element);
            }
            foreach (TContent element in frozenAddElements)
            {
                AddToOther(element);
            }
            frozenRemoveElements.Clear();
            frozenAddElements.Clear();
        }
        public override object Clone()
        {
            return new RelationalCollection<TContent, TThis>(OtherSetProperty, this) { Parent = Parent };
        }
        public bool Frozen { get; private set; } = false;
        private readonly IList<TContent> frozenAddElements = new List<TContent>();
        private readonly IList<TContent> frozenRemoveElements = new List<TContent>();
    }
}
