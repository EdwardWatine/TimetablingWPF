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
using System.IO;

namespace TimetablingWPF
{
    public class Form : BaseDataClass
    {
        public RelationalCollection<Lesson, Form> Lessons { get; private set; } = new RelationalCollection<Lesson, Form>("Forms");
        private YearGroup _year;
        public YearGroup YearGroup
        {
            get { return _year; }
            set
            {
                if (value != _year)
                {
                    _year?.Forms.Remove(this);
                    value.Forms.Add(this);
                    _year = value;
                    NotifyPropertyChanged("YearGroup");
                }
            }
        }
    }
}