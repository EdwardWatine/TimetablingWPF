using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace TimetablingWPF
{
    public enum ErrorType
    {
        Critical = 0,
        Error = 1,
        Warning = 2
    }

    public class ErrorData
    {
        public object Data { get; set; }
    }

    public class ErrorContainer
    {
        public event RoutedEventHandler StateChanged;
        public ErrorContainer(Func<ErrorData, bool> errorFunc, Func<ErrorData, string> messageFunc, ErrorType errorType)
        {
            ErrorType = errorType;
            MessageFunc = messageFunc;
            ErrorFunc = errorFunc;
        }
        public string GetMessage()
        {
            return MessageFunc(errorData);
        }
        public bool UpdateError()
        {
            errorData = new ErrorData();
            ErrorState = ErrorFunc(errorData);
            return ErrorState;
        }
        public void BindCollection(INotifyCollectionChanged collection)
        {
            collection.CollectionChanged += delegate (object o, NotifyCollectionChangedEventArgs e) { UpdateError(); };
        }
        public void BindProperty(INotifyPropertyChanged item, string property)
        {
            if (item.GetType().GetProperty(property) == null)
            {
                throw new InvalidOperationException($"Property '{property}' not found on item of type '{item.GetType().Name}'");
            }
            item.PropertyChanged += delegate (object o, PropertyChangedEventArgs e) { if (property == e.PropertyName) { UpdateError(); } };
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1030:Use events where appropriate")]
        protected virtual void RaiseStateChanged()
        {
            StateChanged?.Invoke(this, new RoutedEventArgs());
        }

        public ErrorType ErrorType { get; }
        private readonly Func<ErrorData, string> MessageFunc;
        private readonly Func<ErrorData, bool> ErrorFunc;
        private ErrorData errorData;
        private bool state = false;
        public bool ErrorState
        {
            get => state;
            private set
            {
                if (state ^ value)
                {
                    state = value;
                    RaiseStateChanged();
                }
            }
        }
    }
    public class ErrorManager
    {
        public ErrorManager(Panel parent)
        {
            Parent = parent;
        }
        public void AddError(ErrorContainer error)
        {
            if (Errors.ContainsKey(error))
            {
                return;
            }
            ErrorType errorType = error.ErrorType;
            Grid gd = new Grid()
            {
                VerticalAlignment = VerticalAlignment.Top,
                Visibility = Visibility.Collapsed
            };
            gd.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
            gd.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
            gd.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            ImageSource source;
            SolidColorBrush colour;
            switch (errorType)
            {
                case ErrorType.Warning:
                    source = (ImageSource)Application.Current.Resources["WarningIcon"];
                    colour = (SolidColorBrush)Application.Current.Resources["WarningBrush"];
                    break;
                case ErrorType.Error:
                    source = (ImageSource)Application.Current.Resources["ErrorIcon"];
                    colour = (SolidColorBrush)Application.Current.Resources["ErrorBrush"];
                    break;
                default:
                    colour = (SolidColorBrush)Application.Current.Resources["ErrorBrush"];
                    source = (ImageSource)Application.Current.Resources["CriticalIcon"];
                    break;
            }
            Image im = new Image()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                Source = source,
                Height = 50
            };
            gd.Children.Add(im);
            TextBlock tb = new TextBlock()
            {
                Foreground = colour,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };
            gd.Tag = tb;
            gd.Insert(tb, 0, 1);
            Errors[error] = gd;
            Parent.Children.Add(gd);
            gd.SetBinding(FrameworkElement.WidthProperty, new Binding("ActualWidth") { Source = Parent, Mode = BindingMode.OneWay });
            error.StateChanged += delegate (object sender, RoutedEventArgs e) { UpdateError(error); };
        }
        public void UpdateError(ErrorContainer error)
        {
            Grid gd = Errors[error];
            bool state = error.ErrorState;
            gd.Visibility = state ? Visibility.Visible : Visibility.Collapsed;
            if (state)
            {
                ((TextBlock)gd.Tag).Text = state ? error.GetMessage() : string.Empty;
            }
        }
        public void UpdateAll()
        {
            foreach (ErrorContainer error in Errors.Keys)
            {
                UpdateError(error);
            }
        }
        private readonly Dictionary<ErrorContainer, Grid> Errors = new Dictionary<ErrorContainer, Grid>();
        public int CountErrors(ErrorType errorType)
        {
            int count = 0;
            foreach (ErrorContainer error in Errors.Keys)
            {
                if (error.ErrorType == errorType)
                {
                    count += error.UpdateError() ? 1 : 0;
                }
            }
            return count;
        }
        private readonly Panel Parent;
    }
}
