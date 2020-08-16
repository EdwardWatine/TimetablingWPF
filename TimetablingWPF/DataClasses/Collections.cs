using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ObservableComputations;

namespace TimetablingWPF
{
    /// <summary>
    /// A list which reflects updates in itself with the list in the added class
    /// </summary>
    /// <typeparam name="TContent">The type of the objects in this list</typeparam>
    /// <typeparam name="TThis">The type of the object declaring this list</typeparam>
    public class RelationalCollection<TContent, TThis> : ObservableCollectionExtended<TContent>, IRelationalCollection, IFreezable
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
            AddParentToOther(item);
        }
        protected virtual void OnlyInsert(int index, TContent item)
        {
            if (_parent == null)
            {
                throw new InvalidOperationException("Parent is not set");
            }
            base.InsertItem(index, item);
        }
        public void AddParentToOther(TContent item)
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
            TContent item = this[index];
            base.RemoveItem(index);
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
            AddParentToOther(item);
        }
        protected virtual void OnlyRemove(int index)
        {
            base.RemoveItem(index);
        }
        public void RemoveFromOther(TContent item)
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
                AddParentToOther(frozenAddElements.Dequeue()); // unfreeze and add the elements
            }
            while (frozenRemoveElements.Count > 0)
            {
                RemoveFromOther(frozenRemoveElements.Dequeue()); // unfreeze and remove the elements
            }
        }
        public bool Frozen { get; private set; } = false;
        private readonly Queue<TContent> frozenAddElements = new Queue<TContent>();
        private readonly Queue<TContent> frozenRemoveElements = new Queue<TContent>();
    }
}
