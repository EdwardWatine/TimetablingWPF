using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimetablingWPF
{
    public abstract class BaseDataProvider<T> : INotifyPropertyChanged
    {
        public abstract T Value { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
    public class DataProvider<T> : BaseDataProvider<T>
    {
        public DataProvider(T defaultValue) : this(new DataWrapper<T>(defaultValue)) { }
        public DataProvider(BaseDataProvider<T> defaultProvider)
        {
            DefaultProvider = defaultProvider;
            defaultProvider.PropertyChanged += DefaultChanged;
        }
        private T value = default;
        public bool UsingDefault { get; private set; } = true;
        public override T Value
        {
            get
            {
                if (UsingDefault)
                {
                    return DefaultProvider.Value;
                }
                return value;
            }
            set
            {
                UsingDefault = false;
                if (!value?.Equals(this.value) ?? this.value != null)
                {
                    this.value = value;
                    RaisePropertyChanged(nameof(Value));
                }
            }
        }
        public void UseDefault()
        {
            UsingDefault = true;
            if (!DefaultProvider.Value?.Equals(value) ?? value != null)
            {
                RaisePropertyChanged(nameof(Value));
            }
        }
        public BaseDataProvider<T> DefaultProvider { get; }
        private void DefaultChanged(object sender, PropertyChangedEventArgs e)
        {
            if (UsingDefault)
            {
                RaisePropertyChanged(nameof(Value));
            }
        }
    }
    public class DataWrapper<T> : BaseDataProvider<T>
    {
        public DataWrapper(T value)
        {
            Value = value;
        }
        public DataWrapper()
        {
            Value = default;
        }
        private T value;
        public override T Value { 
            get => value;
            set
            {
                if (!value?.Equals(this.value) ?? this.value != null)
                {
                    this.value = value;
                    RaisePropertyChanged(nameof(Value));
                }
            }
        }
    }
}
