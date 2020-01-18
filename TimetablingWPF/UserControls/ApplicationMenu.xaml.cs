using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for ApplicationMenu.xaml
    /// </summary>
    public partial class ApplicationMenu : UserControl
    {
        public ApplicationMenu()
        {
            InitializeComponent();
            Loaded += ApplicationMenu_Loaded;
        }

        private void ApplicationMenu_Loaded(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            window.CommandBindings.Add(new CommandBinding(ApplicationCommands.New, NewFile));
            window.CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, OpenFile));
            window.CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, SaveFile));
            window.CommandBindings.Add(new CommandBinding(ApplicationCommands.SaveAs, SaveAs));
            window.InputBindings.Add(new InputBinding(ApplicationCommands.SaveAs, new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift)));
        }

        public void NewFile(object sender, ExecutedRoutedEventArgs e)
        {
            VisualHelpers.ShowErrorBox("New");
        }
        public void SaveFile(object sender, ExecutedRoutedEventArgs e)
        {
            string fpath = (string)Application.Current.Properties["CURRENT_FILE_PATH"];
            if (fpath != null)
            {
                FileHelpers.SaveData(fpath);
                return;
            }
            SaveAs(sender, e);
        }
        public void SaveAs(object sender, ExecutedRoutedEventArgs e)
        {
            string fpath = FileHelpers.SaveFileDialogHelper();
            if (fpath != null)
            {
                FileHelpers.SaveData(fpath);
                FileHelpers.RegisterOpenFile(fpath);
            }
        }
        public void OpenFile(object sender, ExecutedRoutedEventArgs e)
        {
            string fpath = FileHelpers.OpenFileDialogHelper();
            if (fpath != null && fpath != (string)Application.Current.Properties["CURRENT_FILE_PATH"])
            {
                DataHelpers.ClearData();
                FileHelpers.LoadData(fpath, (worker_args) => { if (!worker_args.Cancelled) { FileHelpers.RegisterOpenFile(fpath); } }, Window.GetWindow(this));
            }
        }
    }

    public static class MenuCommands
    {
        static RoutedUICommand ImportCommand = new RoutedUICommand("Import", "Import", typeof(MenuCommands),
            new InputGestureCollection() {
                new KeyGesture(Key.I, ModifierKeys.Control)
            });
    }
}