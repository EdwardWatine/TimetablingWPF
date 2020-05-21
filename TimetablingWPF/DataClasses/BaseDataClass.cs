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
using TimetablingWPF.Searching;
using TimetablingWPF;

namespace TimetablingWPF
{
    /// <summary>
    /// Base class for all data objects
    /// </summary>
    /// 

    public abstract class BaseDataClass : INotifyPropertyChanged, ICloneable, IFreezable, ISaveable
    {

        public BaseDataClass()
        {
            ApplyOnType<IRelationalCollection>((prop, val) => val.Parent = this);  //Links to all the RelationalCollections on instance creation
            void SubscribeToCollectionChange(PropertyInfo prop, INotifyCollectionChanged val) //Propagates changes in internal collections
            {
                void Val_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
                {
                    if (e.IsNotPropertyChanged() && !Frozen) //Ensures that the change is propagated if it wasn't propagated by another object
                    {
                        NotifyPropertyChanged(prop.Name);
                    }
                    ///Debug.WriteLine($"{Name} (a {GetType().Name}) registered a change in property {prop.Name} caused by {e.Action}");
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
                    NotifyPropertyChanged(nameof(Name));
                }
            }
        }
        /// <summary>
        /// Holder for Name
        /// </summary>
        private string _name;

        public string Shorthand
        {
            get => _sh;
            set
            {
                if (value != _sh)
                {
                    _sh = value;
                    NotifyPropertyChanged(nameof(Shorthand));
                }
            }
        }
        private string _sh;
        public bool Commited { get; private set; } = false;
        public bool Frozen { get; private set; } = false;
        public int StorageIndex { get; set; }
        public static Dictionary<Type, IList<CustomPropertyInfo>> ExposedProperties { get; } = new Dictionary<Type, IList<CustomPropertyInfo>>();
        protected static void RegisterProperty(Type declaringType, string property, string alias = null, Func<object, string> display = null)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            ExposedProperties.DefaultDictGet<Type, IList<CustomPropertyInfo>, List<CustomPropertyInfo>>(declaringType, out IList<CustomPropertyInfo> list);
            list.Add(new CustomPropertyInfo(declaringType, property, alias, display));
        }
        public static Dictionary<Type, IList<SearchBase>> SearchParameters { get; } = new Dictionary<Type, IList<SearchBase>>();
        protected static void AddSearchParameter(Type declaringType, SearchBase searchParameter)
        {
            SearchParameters.DefaultDictGet<Type, IList<SearchBase>, List<SearchBase>>(declaringType, out IList<SearchBase> list);
            list.Add(searchParameter);
        }
        /// <summary>
        /// Add this to its associated list in properties. Is idempotent.
        /// </summary>
        public virtual void Commit(DataContainer container = null)
        {
            if (!Commited)
            {
                container = container ?? DataHelpers.GetDataContainer();
                container.AddFromBDC(this);
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
            foreach (CustomPropertyInfo prop in ExposedProperties[type])
            {
                prop.PropertyInfo.SetValue(this, prop.PropertyInfo.GetValue(clone));
                NotifyPropertyChanged(prop.PropertyInfo.Name);
            }
            Name = clone.Name;
            Frozen = clone.Frozen;
        }
        public void MergeWith(BaseDataClass merger)
        {
            Type type = GetType();
            if (type != merger.GetType())
            {
                throw new ArgumentException("The merger class must be the same as the calling class.");
            }
            ApplyOnType<IAddRange>((prop, val) => val.AddRange((IEnumerable)prop.GetValue(merger)));
        }
        /// <summary>
        /// Event when property is changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string property)
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
        public virtual void Delete(DataContainer container = null)
        {
            void delete(PropertyInfo prop, IRelationalCollection val)
            {
                foreach (object @object in (IEnumerable)val)
                {
                    ((IList)@object.GetType().GetProperty(val.OtherSetProperty).GetValue(@object)).Remove(this);
                }
            }
            ApplyOnType<IRelationalCollection>(delete);
            (container ?? DataHelpers.GetDataContainer()).FromType(GetType()).Remove(this);
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

        public virtual void Freeze()
        {
            if (Frozen) { return; }
            Frozen = true;
            ApplyOnType<IFreezable>((prop, val) => val.Freeze());
        }
        public virtual void Unfreeze()
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
        public abstract void Save(BinaryWriter writer);
        public abstract void Load(BinaryReader reader, Version version, DataContainer container);
        protected void LoadParent(BinaryReader reader, Version version, DataContainer container)
        {
            Name = reader.ReadString();
        }
        protected void SaveParent(BinaryWriter writer)
        {
            writer.Write(Name);
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
        public abstract IEnumerable<ErrorContainer> ErrorValidations { get; }
    }
}
