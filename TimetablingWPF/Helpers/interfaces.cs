using System;
using System.Collections;
using System.Collections.Generic;
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
        public ErrorStateChangedEventArgs(bool errorState, ErrorType errorType)
        {
            ErrorState = errorState;
            ErrorType = errorType;
        }
        public bool ErrorState { get; }
        public ErrorType ErrorType { get; }
    }
    public delegate void ErrorStateChangedEventHandler(object sender, ErrorStateChangedEventArgs e);
    public interface INotifyErrorStateChanged
    {
        event ErrorStateChangedEventHandler ErrorStateChanged;
    }
}
