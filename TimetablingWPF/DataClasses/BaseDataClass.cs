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

namespace TimetablingWPF
{
    /// <summary>
    /// Base form for all data objects
    /// </summary>
    public abstract class BaseDataClass : INotifyPropertyChanged, ICloneable, IFreezable
    {

        public BaseDataClass()
        {
            ApplyOnType<IRelationalCollection>((prop, val) => val.Parent = this);
            void SubscribeToCollectionChange(PropertyInfo prop, INotifyCollectionChanged val)
            {
                void Val_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
                {
                    if (e.NewItems != null && e.OldItems != null && e.NewItems[0] != e.OldItems[0] && !((IFreezable)sender).Frozen)
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
            get { return _name; }
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
        public ushort StorageIndex { get; set; }
        /// <summary>
        /// Add this to its associated list in properties. Is idempotent.
        /// </summary>
        public virtual void Commit()
        {
            if (Commited)
            {
                return;
            }
            ((IList)Application.Current.Properties[GetType()]).Add(this);
            Commited = true;
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
            return ReferenceEquals(this, obj) || ReferenceEquals(obj, Wildcard);
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

            copy.ApplyOnType<ICloneable>((prop, val) => prop.SetValue(copy, val.Clone()));
            copy.ApplyOnType<IRelationalCollection>((prop, val) => val.Parent = this);
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

        private void ApplyOnType<T>(Action<PropertyInfo, T> action)
        {
            foreach (PropertyInfo prop in GetType().GetProperties())
            {
                object val = prop.GetValue(this);
                if (val is T)
                {
                    action(prop, (T)val);
                }
            };
        }

        public static bool operator ==(BaseDataClass left, object right)
        {
            if (left is null)
            {
                if (right is null)
                {
                    return true;
                }
                return false;
            }
            return left.Equals(right);
        }
        public static bool operator !=(BaseDataClass left, object right)
        {
            return !(left == right);
        }
    }
}
