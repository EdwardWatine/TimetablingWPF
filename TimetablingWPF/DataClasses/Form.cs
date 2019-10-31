using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
using System.Diagnostics;
using System.Collections.Specialized;

namespace TimetablingWPF
{
    public class Form : BaseDataClass
    {
        public ObservableCollection<Lesson> Lessons { get; private set; } = new ObservableCollection<Lesson>();
        public ObservableCollection<Assignment> Assignments { get; private set; } = new ObservableCollection<Assignment>();
        public const string ListName = "Forms";
        protected override string ListNameAbstract => ListName;
    }
}