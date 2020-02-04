using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TimetablingWPF
{
    public class WindowBase : Window
    {
        public WindowBase()
        {
            Closing += delegate (object sender, CancelEventArgs e) 
            { 
                if (Application.Current.Windows.Count == 1 && DataHelpers.GetDataContainer().Unsaved)
                {
                    MessageBoxResult result = VisualHelpers.ShowUnsavedBox();
                    if (result == MessageBoxResult.Yes)
                    {
                        FileHelpers.SaveData(FileHelpers.GetCurrentFilePath());
                    }
                    if (result == MessageBoxResult.Cancel)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            };
        }
    }
}
