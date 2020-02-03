﻿using System;
using System.Collections;
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
using static TimetablingWPF.DataHelpers;
using static TimetablingWPF.FileHelpers;

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
            window.CommandBindings.Add(new CommandBinding(MenuCommands.ImportCommand, ImportFile));
            window.CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, SaveFile));
            window.CommandBindings.Add(new CommandBinding(ApplicationCommands.SaveAs, SaveAs));
            window.InputBindings.Add(new InputBinding(ApplicationCommands.SaveAs, new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift)));
        }

        public void NewFile(object sender, ExecutedRoutedEventArgs e)
        {
            string fpath = SaveFileDialogHelper("Create New File");
            if (fpath != null)
            {
                TimetableStructureDialog structureDialog = new TimetableStructureDialog(Window.GetWindow(this), false);
                if (!structureDialog.ShowDialog() ?? false) { return; }
                ClearData();
                SaveData(fpath);
                RegisterOpenFile(fpath);
            }
        }
        public void SaveFile(object sender, ExecutedRoutedEventArgs e)
        {
            string fpath = GetCurrentFilePath();
            if (fpath != null)
            {
                SaveData(fpath);
                return;
            }
            SaveAs(sender, e);
        }
        public void SaveAs(object sender, ExecutedRoutedEventArgs e)
        {
            string fpath = SaveFileDialogHelper();
            if (fpath != null)
            {
                SaveData(fpath);
                RegisterOpenFile(fpath);
            }
        }
        public void OpenFile(object sender, ExecutedRoutedEventArgs e)
        {
            string fpath = OpenFileDialogHelper();
            if (fpath != null && fpath != GetCurrentFilePath())
            {
                ClearData();
                LoadData(fpath, worker_args => RegisterOpenFile(fpath), owner: Window.GetWindow(this));
            }
        }
        public void ImportFile(object sender, ExecutedRoutedEventArgs e)
        {
            string fpath = OpenFileDialogHelper();
            if (fpath != null && fpath != GetCurrentFilePath())
            {
                ImportDialog window = new ImportDialog(Window.GetWindow(this));
                window.ShowDialog();
                if (!window.DialogResult ?? false) return; 
                LoadData(fpath, (complete) =>
                {
                    DataContainer dataContainer = (DataContainer)complete.Result;
                    DataContainer currentContainer = GetDataContainer();
                    Dictionary<Type, ImportOption> typeImportMapping = new Dictionary<Type, ImportOption>()
                    {
                        {typeof(Teacher), window.ImportTeacher },
                        {typeof(Group), window.ImportGroup },
                        {typeof(Subject), window.ImportSubject },
                        {typeof(Room), window.ImportRoom },
                        {typeof(Form), window.ImportForm },
                        {typeof(Lesson), window.ImportLesson },
                    };
                    if (window.ImportAssignment == ImportOption.NoImport || window.ImportTeacher == ImportOption.NoImport || window.ImportLesson == ImportOption.NoImport)
                    {
                        foreach (Teacher teacher in currentContainer.Teachers)
                        {
                            teacher.Assignments.Clear();
                        }
                    }
                    if (window.ImportTimetable == ImportOption.Import) TimetableStructure.SetData(dataContainer.TimetableStructure);
                    foreach (KeyValuePair<Type, ImportOption> mapping in typeImportMapping)
                    {
                        if (mapping.Value == ImportOption.Import)
                        {
                            ((IAddRange)currentContainer.FromType(mapping.Key)).AddRange(dataContainer.FromType(mapping.Key).Cast<object>());
                            continue;
                        }
                        if (mapping.Value == ImportOption.Replace || mapping.Value == ImportOption.Merge)
                        {
                            foreach (BaseDataClass currentdata in currentContainer.FromType(mapping.Key).Cast<BaseDataClass>())
                            {
                                Dictionary<string, BaseDataClass> nameMapping = dataContainer.FromType(mapping.Key).Cast<BaseDataClass>().ToDictionary(bdc => bdc.Name);
                                if (!nameMapping.TryGetValue(currentdata.Name, out BaseDataClass similar)) {
                                    foreach (BaseDataClass target in nameMapping.Values)
                                    {
                                        int max = currentdata.Name.Length > target.Name.Length ? currentdata.Name.Length: target.Name.Length;
                                        if (GenericHelpers.DamerauLevenshteinDistance(currentdata.Name, target.Name, max * 3 / 8) != int.MaxValue)
                                        {
                                            if (MessageBox.Show($"'{currentdata.Name}' is similar to '{target.Name}'. Import '{currentdata.Name}'?", "Import?", 
                                                MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                                            {
                                                similar = currentdata;
                                                break;
                                            }
                                        }
                                    }
                                }
                                if (similar != null)
                                {
                                    if (mapping.Value == ImportOption.Replace)
                                    {
                                        IList list = currentContainer.FromType(mapping.Key);
                                        list[list.IndexOf(similar)] = currentdata;
                                        continue;
                                    }
                                    similar.MergeWith(currentdata);
                                    continue;
                                }
                                currentdata.Delete(currentContainer);
                                continue;
                            }
                            foreach (BaseDataClass dataClass in currentContainer.FromType(mapping.Key))
                            {
                                dataClass.Delete(currentContainer);
                            }
                        }
                    }
                    
                }, owner: Window.GetWindow(this), save: false);
            }
        }
        public void BlockingViewClick(object sender, RoutedEventArgs e)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is BlockingWindow)
                {
                    window.Activate();
                    return;
                }
            }
            new BlockingWindow().Show();
        }
    }
    public static class MenuCommands
    {
        public static readonly RoutedUICommand ImportCommand = new RoutedUICommand("Import", "Import", typeof(MenuCommands),
            new InputGestureCollection() {
                new KeyGesture(Key.I, ModifierKeys.Control)
            });
    }
}
