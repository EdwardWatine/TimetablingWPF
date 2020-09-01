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

    public abstract class BaseDataClass : IDeleteable, IFreezable, ISaveable, INotifyErrorStateChanged, INotifyPropertyChanged
    {

        public BaseDataClass()
        {
            ApplyOnType<IRelationalCollection>((prop, val) => val.Parent = this);  //Links to all the RelationalCollections on instance creation
            ApplyOnType<INotifyCollectionChanged>(SubscribeToCollectionChange);
        }
        private void SubscribeToCollectionChange(PropertyInfo prop, INotifyCollectionChanged val) //Propagates changes in internal collections
        {
            void Val_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                if (e.IsAddOrRemove() && !Frozen) //Ensures that the change is propagated if it wasn't propagated by another object
                {
                    NotifyPropertyChanged(prop.Name);
                }
                //Debug.WriteLine($"{Name} (a {GetType().Name}) registered a change in property {prop.Name} caused by {e.Action}");
            }
            val.CollectionChanged += Val_CollectionChanged;
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
        public bool Visible { get; set; } = true;
        public bool Committed { get; private set; } = false;
        public bool Frozen { get; private set; } = false;
        public int StorageIndex { get; set; }
        public static Dictionary<Type, IList<CustomPropertyInfo>> ExposedProperties { get; } = new Dictionary<Type, IList<CustomPropertyInfo>>();
        protected static void RegisterProperty(Type declaringType, string property, string alias = null, Func<object, string> display = null)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            ExposedProperties.DefaultDictGet<Type, IList<CustomPropertyInfo>, List<CustomPropertyInfo>>(declaringType, out IList<CustomPropertyInfo> list);
            list.Add(new CustomPropertyInfo(declaringType, property, alias, display));
        }
        public static Dictionary<Type, IList<SearchFactory>> SearchParameters { get; } = new Dictionary<Type, IList<SearchFactory>>();
        protected static void AddSearchParameter(Type declaringType, SearchFactory searchFactory)
        {
            SearchParameters.DefaultDictGet<Type, IList<SearchFactory>, List<SearchFactory>>(declaringType, out IList<SearchFactory> list);
            list.Add(searchFactory);
        }
        /// <summary>
        /// Add this to its associated list in properties. Is idempotent.
        /// </summary>
        public virtual void Commit(DataContainer container = null)
        {
            if (!Committed)
            {
                container = container ?? App.Data;
                container.AddFromBDC(this);
                Committed = true;
            }
        }
        public void UpdateFrom(BaseDataClass source)
        {
            Type type = GetType();
            if (type != source.GetType())
            {
                throw new ArgumentException("The clone class must be of the same type as the calling class.");
            }
            Name = source.Name;
            Shorthand = source.Shorthand;
            foreach (CustomPropertyInfo prop in ExposedProperties[type])
            {
                object value = prop.PropertyInfo.GetValue(source);
                if (value is IRelationalCollection rc)
                {
                    rc.Parent = this;
                }
                if (value is IList)
                {
                    ((IList)prop.PropertyInfo.GetValue(this)).SetData((IEnumerable)value);
                    continue;
                }
                prop.PropertyInfo.SetValue(this, value);
            }
        }
        public BaseDataClass Clone()
        {
            BaseDataClass copy = (BaseDataClass)Activator.CreateInstance(GetType());
            copy.UpdateFrom(this);
            return copy;
        }
        public void MergeFrom(BaseDataClass merger)
        {
            Type type = GetType();
            if (type != merger.GetType())
            {
                throw new ArgumentException("The merger class must be the same as the calling class.");
            }
            ApplyOnType<IList>((prop, val) => val.AddRange((IEnumerable)prop.GetValue(merger)));
        }
        /// <summary>
        /// Event when property is changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        public event ErrorStateChangedEventHandler ErrorStateChanged;
        public event EventHandler Deleted;
        private void ErrorsChanged(ErrorStateChangedEventArgs e)
        {
            ErrorStateChanged?.Invoke(e.AppendObject(this));
        }
        protected virtual void NotifyPropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public override string ToString()
        {
            return Name;
        }
        /// <summary>
        /// Will remove all instances of self from <see cref="RelationalCollection{T}"/>. Will then remove self from the properties list
        /// </summary>
        public virtual void Delete(DataContainer container)
        {
            void delete(PropertyInfo prop, IRelationalCollection val)
            {
                foreach (object @object in (IEnumerable)val)
                {
                    ((IList)@object.GetType().GetProperty(val.OtherSetProperty).GetValue(@object)).Remove(this);
                }
            }
            ApplyOnType<IRelationalCollection>(delete);
            (container ?? App.Data).FromType(GetType()).Remove(this);
            Deleted?.Invoke(this, EventArgs.Empty);
        }
        public virtual void Delete()
        {
            Delete(null);
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
        public abstract void SaveChild(BinaryWriter writer);
        public abstract void LoadChild(BinaryReader reader, Version version, DataContainer container);
        public void Load(BinaryReader reader, Version version, DataContainer container)
        {
            Name = reader.ReadString();
            Shorthand = reader.ReadString();
            Visible = reader.ReadBoolean();
            LoadChild(reader, version, container);
        }
        public void Save(BinaryWriter writer)
        {
            writer.Write(Name);
            writer.Write(Shorthand);
            writer.Write(Visible);
            SaveChild(writer);
        }
        protected List<ErrorContainer> ErrorList { get; } = new List<ErrorContainer>();
        protected void BindToErrors()
        {
            foreach (ErrorContainer error in ErrorList)
            {
                error.ErrorStateChanged += ErrorsChanged;
            }
        }
        public int GetErrorCount(ErrorType errorType)
        {
            int count = 0;
            foreach (ErrorContainer error in ErrorList)
            {
                if (error.ErrorType == errorType)
                {
                    count += error.UpdateError() ? 1 : 0;
                }
            }
            return count;
        }
        public ReadOnlyCollection<ErrorContainer> ErrorValidations => ErrorList.AsReadOnly();
    }
}
