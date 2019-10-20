using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimetablingWPF
{
    public interface IRelationalCollection
    {
        object Parent { get; set; }
        string OtherClassProperty { get; set; }
    };
    public class ObservableCollection<T> : System.Collections.ObjectModel.ObservableCollection<T>, ICloneable
    {
        public ObservableCollection() { }
        public ObservableCollection(IEnumerable<T> collection) : base(collection) { }

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
    /// A list which reflects updates in itself with the list in the added class
    /// </summary>
    /// <typeparam name="T">The type of the objects in this list</typeparam>
    public class RelationalCollection<T> : InternalObservableCollection<T>, IRelationalCollection where T : INotifyPropertyChanged
    {
        /// <summary>
        /// The object to which this list belongs
        /// </summary>
        public object Parent { get; set; }
        /// <summary>
        /// Holds the field name of the object of type T which holds the RelationalList which will hold the parent
        /// </summary>
        public string OtherClassProperty { get; set; }
        /// <summary>
        /// The class constructor
        /// </summary>
        /// <param name="otherClassProperty"><see cref="OtherClassProperty"/></param>
        /// <param name="parent"><see cref="Parent"/></param>
        public RelationalCollection(string otherClassProperty,
            BaseDataClass parent = null)
        {
            OtherClassProperty = otherClassProperty;
            Parent = parent;
        }

        public RelationalCollection(string otherClassProperty,
            IEnumerable<T> collection, BaseDataClass parent = null) : base(collection)
        {
            OtherClassProperty = otherClassProperty;
            Parent = parent;
        }
        /// <summary>
        /// Adds a new item to the list, and adds the parent in the list of the added item
        /// </summary>
        /// <param name="item">Item to add to the list</param>
        public new void Add(T item)
        {
            if (Parent == null)
            {
                throw new InvalidOperationException("Parent is not set");
            }
            base.Add(item);

            ((IList)item.GetType().GetProperty(OtherClassProperty).GetValue(item)).Add(Parent);
        }
        public new void Remove(T item)
        {
            if (Parent == null)
            {
                throw new InvalidOperationException("Parent is not set");
            }
            base.Remove(item);
            ((IList)item.GetType().GetProperty(OtherClassProperty).GetValue(item)).Remove(Parent);
        }
        public override object Clone()
        {
            return new RelationalCollection<T>(OtherClassProperty, this) { Parent = Parent };
        }
    }
}
