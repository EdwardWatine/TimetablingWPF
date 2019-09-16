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
        public Error(string message, ErrorType errorType)
        {
            ErrorType = errorType;
            IsError = false;
            Message = message;
        }
        public ErrorType ErrorType { get; }
        public bool IsError;
        public string Message { get; set; }
    }
    class ErrorManager
    {
        public ErrorManager(Panel parent)
        {
            Parent = parent;
        }
        public void AddError(Error error, bool isError = false)
        {
            ErrorType errorType = error.ErrorType;
            if (error.IsError)
            {
                AddNum(true, error.ErrorType);
            }
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
                    Text = error.Message,
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
        public void UpdateError(Error error, bool isError)
        {
            StackPanel sp = Errors[error];
            if (error.IsError^isError)
            {
                error.IsError = isError;
                if (isError)
                {
                    sp.Visibility = Visibility.Visible;
                }
                else
                {
                    sp.Visibility = Visibility.Collapsed;
                }
                AddNum(isError, error.ErrorType);
            }
        }
        private void AddNum(bool isError, ErrorType errorType)
        {
            int change = isError ? 1 : -1;
            switch (errorType)
            {
                case ErrorType.Warning:
                    NumWarnings += change;
                    break;
                default:
                    NumErrors += change;
                    break;
            }
        }
        private Dictionary<Error, StackPanel> Errors = new Dictionary<Error, StackPanel>();
        private int NumErrors = 0;
        public int GetNumErrors() { return NumErrors; }
        private int NumWarnings = 0;
        public int GetNumWarnings() { return NumWarnings; }
        private Panel Parent;
    }
}
