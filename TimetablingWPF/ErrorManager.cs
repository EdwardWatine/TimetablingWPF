using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TimetablingWPF
{
    public enum ErrorType : byte {
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
            StackPanel sp = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Height = 50,
                Visibility = state ? Visibility.Visible : Visibility.Collapsed
            };
            ImageSource source;
            SolidColorBrush colour;
            switch (errorType)
            {
                case ErrorType.Warning:
                    source = (ImageSource)Application.Current.Resources["WarningIcon"];
                    colour = (SolidColorBrush)new BrushConverter().ConvertFromString("#FC9803");
                    break;
                default:
                    source = (ImageSource)Application.Current.Resources["ErrorIcon"];
                    colour = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF0000");
                    break;
            }
            sp.Children.Add(new Image()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                Source = source
            });
            TextBlock tb = new TextBlock()
            {
                Text = state ? error.GetMessage() : string.Empty,
                Foreground = colour,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };
            sp.Tag = tb;
            sp.Children.Add(new ScrollViewer()
            {
                Content = tb,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            });
            Errors[error] = sp;
            Parent.Children.Add(sp);
        }
        public void UpdateError(ErrorContainer error, bool state)
        {
            StackPanel sp = Errors[error];
            sp.Visibility = state ? Visibility.Visible : Visibility.Collapsed;
            if (state)
            {
                ((TextBlock)sp.Tag).Text = state ? error.GetMessage() : string.Empty;
            }
        }
        private readonly Dictionary<ErrorContainer, StackPanel> Errors = new Dictionary<ErrorContainer, StackPanel>();
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
