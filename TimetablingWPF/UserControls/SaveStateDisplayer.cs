using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TimetablingWPF
{
    /// <summary>
    /// Interaction logic for SaveStateDisplayer.xaml
    /// </summary>
    public class SaveStateDisplayer : TextBlock, IDisposable
    {
        public SaveStateDisplayer()
        {
            timer = new Timer(delay);
            timer.Elapsed += Animate;
            timer.AutoReset = false;
            Text = "ummm, hello?";
            fadeto = VisualHelpers.GenerateDoubleAnimation(1, 0, fade_duration, this, OpacityProperty, new QuadraticEase());
            fadeto.Completed += FadeToDone;
            fadefrom = VisualHelpers.GenerateDoubleAnimation(0, 1, fade_duration, this, OpacityProperty, new QuadraticEase());
            fadefrom.Completed += FadeFromDone;
            App.Data.SaveStateChanged += SaveStateChanged;
            SaveStateChanged();
        }

        private void FadeFromDone(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => timer.Start());
        }

        private void FadeToDone(object sender, EventArgs e)
        {
            if (count == 0)
            {
                SetText($"Last backed up at {App.Data.LastBackup:hh\\:mm}");
            }
            else
            {
                SetText($"Last saved at {App.Data.LastSave:hh\\:mm}");
            }
            count = (count + 1) % 2;
            Dispatcher.Invoke(() => fadefrom.Begin());
        }

        private void Animate(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() => fadeto.Begin());
        }
        private int count = 0;
        private const int delay = 2700;
        private const int fade_duration = 700;
        private readonly Storyboard fadeto = new Storyboard();
        private readonly Storyboard fadefrom = new Storyboard();
        private readonly Timer timer;
        private void SetText(string text)
        {
            Dispatcher.Invoke(() => Text = text);
        }
        private void SaveStateChanged()
        {
            if (!App.Data.Unsaved)
            {
                count = 0;
                timer.Stop();
                fadeto.Stop();
                SetText("Save file up to date!");
                return;
            }
            SetText($"Last saved at {App.Data.LastSave:hh\\:mm}");
            if (App.Data.LastBackup != null)
            {
                Animate(null, null); // Just looks cooler than engaging timer. 
                                        // I suppose that the entry point doesn't matter in a cyclical calling structure ^_^
            }
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
