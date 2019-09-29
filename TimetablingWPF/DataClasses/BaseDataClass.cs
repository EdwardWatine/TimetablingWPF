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

namespace TimetablingWPF
{
    /// <summary>
    /// Base class for all data objects
    /// </summary>
    public abstract class BaseDataClass : INotifyPropertyChanged, ICloneable
    {

        public BaseDataClass()
        {
            ApplyOnType<IRelationalList>((prop, val) => val.Parent = this);
            void SubscribeToCollectionChange(System.Reflection.PropertyInfo prop, INotifyCollectionChanged val)
            {
                void Val_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
                {
                    if (e.Action != NotifyCollectionChangedAction.Replace)
                    {
                        NotifyPropertyChanged(prop.Name);
                        return;
                    }
                }
                val.CollectionChanged += Val_CollectionChanged;
            }
            

            ApplyOnType<INotifyCollectionChanged>(SubscribeToCollectionChange);
        }

        public string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }
        /// <summary>
        /// Holder for Name
        /// </summary>
        private string _Name;
        protected abstract string ListNameAbstract { get; }
        private bool Commited = false;
        /// <summary>
        /// Add this to its associated list in properties. Is idempotent.
        /// </summary>
        public virtual void Commit()
        {
            if (Commited)
            {
                return;
            }
            ((IList)Application.Current.Properties[ListNameAbstract]).Add(this);
            Commited = true;
        }
        public void Recommit(BaseDataClass replace)
        {
            int index = ((IList)Application.Current.Properties[ListNameAbstract]).IndexOf(this);
            ((IList)Application.Current.Properties[ListNameAbstract])[index] = replace;
            Delete();
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
        /// <summary>
        /// Will remove all instances of self from <see cref="RelationalList{T}"/>. Will then remove self from the properties list
        /// </summary>
        public void Delete()
        {
            void delete(System.Reflection.PropertyInfo prop, IRelationalList val)
            {
                foreach (object @object in (IEnumerable)val)
                {
                    ((IList)@object.GetType().GetProperty(val.OtherClassProperty).GetValue(@object)).Remove(this);
                }
            }
            ApplyOnType<IRelationalList>(delete);
            ((IList)Application.Current.Properties[ListNameAbstract]).Remove(this);
        }

        public object Clone()
        {
            BaseDataClass copy = (BaseDataClass)MemberwiseClone();
            copy.Commited = false;
            copy.PropertyChanged = null;

            ApplyOnType<IEnumerable>((prop, val) => prop.SetValue(copy, ((ICloneable)val).Clone()));
            ApplyOnType<IRelationalList>((prop, val) => val.Parent = this);
            return copy;
        }

        private void ApplyOnType<T>(Action<System.Reflection.PropertyInfo, T> action)
        {
            foreach (System.Reflection.PropertyInfo prop in GetType().GetProperties())
            {
                object val = prop.GetValue(this);
                if (val is T)
                {
                    action(prop, (T)val);
                }
            };
        }
    }
}
