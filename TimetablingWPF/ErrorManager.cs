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
        Error = 0,
        Warning = 1
    }

    public class ErrorData
    {
        public object Data { get; set; }
    }

    public class ErrorContainer
    {
        public ErrorContainer(ErrorManager em, Func<ErrorData, bool> errorFunc, Func<ErrorData, string> messageFunc, ErrorType errorType, bool? state = null)
        {
            ErrorType = errorType;
            MessageFunc = messageFunc;
            ErrorFunc = errorFunc;
            ErrManager = em;
            em.AddError(this, state ?? IsTriggered());
        }
        public string GetMessage()
        {
            return MessageFunc(errorData);
        }
        public bool IsTriggered()
        {
            errorData = new ErrorData();
            return ErrorFunc(errorData);
        }
        public void UpdateError()
        {
            ErrManager.UpdateError(this, IsTriggered());
        }
        public void SetErrorState(bool state)
        {
            ErrManager.UpdateError(this, state);
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

        public ErrorType ErrorType { get; }
        private readonly Func<ErrorData, string> MessageFunc;
        private readonly Func<ErrorData, bool> ErrorFunc;
        private readonly ErrorManager ErrManager;
        private ErrorData errorData;
    }
    public class ErrorManager
    {
        public ErrorManager(Panel parent)
        {
            Parent = parent;
        }
        public void AddError(ErrorContainer error, bool state)
        {
            if (Errors.ContainsKey(error))
            {
                return;
            }
            ErrorType errorType = error.ErrorType;
            Grid gd = new Grid()
            {
                VerticalAlignment = VerticalAlignment.Top,
                Visibility = state ? Visibility.Visible : Visibility.Collapsed
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
                default:
                    source = (ImageSource)Application.Current.Resources["ErrorIcon"];
                    colour = (SolidColorBrush)Application.Current.Resources["ErrorBrush"];
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
                Text = state ? error.GetMessage() : string.Empty,
                Foreground = colour,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };
            gd.Tag = tb;
            gd.Insert(tb, 0, 1);
            Errors[error] = gd;
            Parent.Children.Add(gd);
            gd.SetBinding(FrameworkElement.WidthProperty, new Binding("ActualWidth") { Source = Parent, Mode = BindingMode.OneWay });
        }
        public void UpdateError(ErrorContainer error, bool state)
        {
            Grid gd = Errors[error];
            gd.Visibility = state ? Visibility.Visible : Visibility.Collapsed;
            if (state)
            {
                ((TextBlock)gd.Tag).Text = state ? error.GetMessage() : string.Empty;
            }
        }
        private readonly Dictionary<ErrorContainer, Grid> Errors = new Dictionary<ErrorContainer, Grid>();
        public int GetNumErrors()
        {
            return Errors.Sum(kvpair => kvpair.Key.IsTriggered() && kvpair.Key.ErrorType == ErrorType.Error ? 1 : 0);
        }
        public int GetNumWarnings()
        {
            return Errors.Sum(kvpair => kvpair.Key.IsTriggered() && kvpair.Key.ErrorType == ErrorType.Warning ? 1 : 0);
        }
        private readonly Panel Parent;
    }
}
