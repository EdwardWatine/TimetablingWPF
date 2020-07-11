using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TimetablingWPF
{
    /// <summary>
    /// Interaction logic for BackgroundTaskDisplayer.xaml
    /// </summary>
    public class BackgroundTaskDisplayer : TextBlock, IDisposable
    {
        public BackgroundTaskDisplayer()
        {
            BackgroundTaskManager.Tasks.CollectionChanged += TasksChanged;
            timer = new Timer();
            timer.Elapsed += ThreadAction;
            timer.Interval = delay;
            TasksChanged(null, null);
        }
        private string text;
        private int count = 0;
        private readonly Timer timer;
        private const int delay = 70;
        private const int dots = 6;
        private void ThreadAction(object sender, ElapsedEventArgs e)
        {
            SetText(text + new string('.', count));
            count = (count + 1) % dots;
        }
        private void SetText(string value)
        {
            Dispatcher.Invoke(() => Text = value);
        }
        private void TasksChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            count = 0;
            if (BackgroundTaskManager.Tasks.Count == 0)
            {
                timer.Stop();
                SetText("No background tasks currently executing");
                return;
            }
            if (BackgroundTaskManager.Tasks.Count == 1)
            {
                text = BackgroundTaskManager.Tasks.First().Name;
            }
            else
            {
                text = $"{BackgroundTaskManager.Tasks.Count} background tasks executing";
            }
            ThreadAction(null, null);
            timer.Start();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);

        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                timer.Dispose();
            }
        }
    }
}
