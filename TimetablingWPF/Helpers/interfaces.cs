using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TimetablingWPF
{
    public interface IAddRange
    {
        void AddRange(IEnumerable enumerable);
    }
    public interface IRelationalCollection
    {
        object Parent { get; set; }
        string OtherSetProperty { get; set; }
    }

    public interface IFreezable
    {
        bool Frozen { get; }
        void Freeze();
        void Unfreeze();
    }
    public interface ISaveable
    {
        void Save(BinaryWriter writer);
        void Load(BinaryReader reader, Version version, DataContainer container);
    }
    public interface IDataObject : INotifyPropertyChanged
    {
        int StorageIndex { get; set; }
        void Delete(DataContainer container);
        string Name { get; set; }
        bool Committed { get; }
        bool Visible { get; }
    }
    public class ErrorStateChangedEventArgs
    {
        public ErrorStateChangedEventArgs() { }
        public ErrorStateChangedEventArgs(object sender)
        {
            objectChain.Add(sender);
        }
        private IList<object> objectChain = new List<object>();
        public ReadOnlyCollection<object> ObjectChain => new ReadOnlyCollection<object>(objectChain);
        public ErrorStateChangedEventArgs AppendObject(object obj)
        {
            ErrorStateChangedEventArgs args = new ErrorStateChangedEventArgs()
            {
                objectChain = new List<object>(objectChain)
                {
                    obj
                }
            };
            return args;
        }
    }
    public delegate void ErrorStateChangedEventHandler(ErrorStateChangedEventArgs e);
    public interface INotifyErrorStateChanged
    {
        event ErrorStateChangedEventHandler ErrorStateChanged;
    }
}
