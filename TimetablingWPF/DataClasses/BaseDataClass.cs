using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Reflection;
using System.IO;

namespace TimetablingWPF
{
    /// <summary>
    /// Base class for all data objects
    /// </summary>
    /// 

    public abstract class BaseDataClass : INotifyPropertyChanged, ICloneable, IFreezable
    {

        public BaseDataClass()
        {
            ApplyOnType<IRelationalCollection>((prop, val) => val.Parent = this);  //Links to all the RelationalCollections on instance creation
            void SubscribeToCollectionChange(PropertyInfo prop, INotifyCollectionChanged val) //Propagates changes in internal collections
            {
                void Val_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
                {
                    if ((e.NewItems != null || e.OldItems != null) && (e.Action != NotifyCollectionChangedAction.Replace || e.NewItems[0] != e.OldItems[0]) && !Frozen) //Ensures that the change is propagated if it wasn't propagated by another object
                    {
                        NotifyPropertyChanged(prop.Name);
                    }
                }
                val.CollectionChanged += Val_CollectionChanged;
            }
            

            ApplyOnType<INotifyCollectionChanged>(SubscribeToCollectionChange);
        }

        public string Name
        {
            get => _name;
            set
            {
                if (value != _name)
                {
                    _name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }
        /// <summary>
        /// Holder for Name
        /// </summary>
        private string _name;
        private bool Commited = false;
        public const string Wildcard = "Any";
        public bool Frozen { get; private set; } = false;
        public int StorageIndex { get; set; }
        /// <summary>
        /// Add this to its associated list in properties. Is idempotent.
        /// </summary>
        public virtual void Commit()
        {
            if (!Commited)
            {
                Application.Current.Dispatcher.Invoke(()=>((IList)Application.Current.Properties[GetType()]).Add(this)); //Adds itself to the correct data list from the main UI thread to prevent threading issues
                Commited = true;
            }
        }
        public void UpdateWithClone(BaseDataClass clone)
        {
            Type type = GetType();
            if (type != clone.GetType())
            {
                throw new ArgumentException("The clone class must be of the same type as the calling class.");
            }
            foreach (PropertyInfo prop in type.GetProperties())
            {
                if (prop.DeclaringType == type)
                {
                    prop.SetValue(this, prop.GetValue(clone));
                    NotifyPropertyChanged(prop.Name);
                }
            }
        }
        /// <summary>
        /// Event when property is changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public override string ToString()
        {
            return Name;
        }
        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj);
        }
        /// <summary>
        /// Will remove all instances of self from <see cref="RelationalCollection{T}"/>. Will then remove self from the properties list
        /// </summary>
        public void Delete()
        {
            void delete(PropertyInfo prop, IRelationalCollection val)
            {
                foreach (object @object in (IEnumerable)val)
                {
                    ((IList)@object.GetType().GetProperty(val.OtherSetProperty).GetValue(@object)).Remove(this);
                }
            }
            ApplyOnType<IRelationalCollection>(delete);
            ((IList)Application.Current.Properties[GetType()]).Remove(this);
        }

        public object Clone()
        {
            BaseDataClass copy = (BaseDataClass)MemberwiseClone();
            copy.Commited = false;
            copy.PropertyChanged = null;
            copy.Unfreeze();

            copy.ApplyOnType<ICloneable>((prop, val) => prop.SetValue(copy, val.Clone())); //copies all copyable objects
            copy.ApplyOnType<IRelationalCollection>((prop, val) => val.Parent = this); //reassigns the parent of copied lists
            return copy;
        }

        public void Freeze()
        {
            if (Frozen) { return; }
            Frozen = true;
            ApplyOnType<IFreezable>((prop, val) => val.Freeze());
        }
        public void Unfreeze()
        {
            if (!Frozen) { return; }
            Frozen = false;
            ApplyOnType<IFreezable>((prop, val) => val.Unfreeze());
        }

        private void ApplyOnType<T>(Action<PropertyInfo, T> action) //Helper function to apply a function to each property of a certain type on the class
        {
            foreach (PropertyInfo prop in GetType().GetProperties())
            {
                object val = prop.GetValue(this);
                if (val is T casted)
                {
                    action(prop, casted);
                }
            };
        }

        public static bool operator ==(BaseDataClass left, object right)
        {
            if (left is null)
            {
                return right is null;
            }
            return left.Equals(right);
        }
        public static bool operator !=(BaseDataClass left, object right)
        {
            return !(left == right);
        }
    }
}
