using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TimetablingWPF
{
    enum ErrorType : byte {
        Error = 0,
        Warning = 1
    }
    class Error
    {
        public Error(object object1, object object2,
            Func<object, object, bool> errorFunc, 
            Func<object, object, string> messageFunc,
            ErrorType errorType)
        {
            ErrorType = errorType;
            Object1 = object1;
            Object2 = object2;
            MessageFunc = messageFunc;
            ErrorFunc = errorFunc;
        }

        public Error(object object1,
            Func<object, bool> errorFunc,
            Func<object, string> messageFunc,
            ErrorType errorType)
        {
            ErrorType = errorType;
            Object1 = object1;
            Object2 = null;
            MessageFunc = (o1, o2) => messageFunc(o1);
            ErrorFunc = (o1, o2) => errorFunc(o1);
        }

        public string GetMessage()
        {
            return IsTriggered() ? string.Empty : MessageFunc(Object1, Object2);
        }

        public bool IsTriggered()
        {
            return ErrorFunc(Object1, Object2);
        }

        public ErrorType ErrorType { get; }
        private readonly object Object1;
        private readonly object Object2;
        private readonly Func<object, object, string> MessageFunc;
        private readonly Func<object, object, bool> ErrorFunc;
    }
    class ErrorManager
    {
        public ErrorManager(Panel parent)
        {
            Parent = parent;
        }
        private void AddError(Error error)
        {
            if (Errors.ContainsKey(error))
            {
                return;
            }
            ErrorType errorType = error.ErrorType;
            bool isError = error.IsTriggered();
            StackPanel sp = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Height = 50,
                Visibility = isError ? Visibility.Visible : Visibility.Collapsed
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
            sp.Children.Add(new ScrollViewer()
            {
                Content = new TextBlock()
                {
                    Text = error.GetMessage(),
                    Foreground = colour,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                },
                HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            });
            Errors[error] = sp;
            Parent.Children.Add(sp);
        }
        public void ErrorUpdate(Error error)
        {
            AddError(error);
            StackPanel sp = Errors[error];
            sp.Visibility = error.IsTriggered() ? Visibility.Visible : Visibility.Collapsed;
        }
        private readonly Dictionary<Error, StackPanel> Errors = new Dictionary<Error, StackPanel>();
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
