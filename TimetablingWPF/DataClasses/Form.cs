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
using System.Reflection;
using TimetablingWPF;

namespace TimetablingWPF
{
    public class Form : BaseDataClass
    {
        static Form()
        {
            Type type = typeof(Form);
            RegisterProperty(type, "Lessons");
            RegisterProperty(type, "YearGroup");
        }
        public Form()
        {
            ErrorContainer no_year = new ErrorContainer(e => YearGroup == null, e => "No year group has been assigned", ErrorType.Warning);
            errorValidations = new List<ErrorContainer>()
            {
                no_year
            };
        }
        public RelationalCollection<Lesson, Form> Lessons { get; private set; } = new RelationalCollection<Lesson, Form>("Forms");
        private Year _year;
        public Year YearGroup
        {
            get => _year;
            set
            {
                if (value != _year)
                {
                    _year?.Forms.Remove(this);
                    value.Forms.Add(this);
                    _year = value;
                    NotifyPropertyChanged(nameof(YearGroup));
                }
            }
        }

        private readonly IList<ErrorContainer> errorValidations;
        public override IEnumerable<ErrorContainer> ErrorValidations => errorValidations;

        public override void Save(BinaryWriter writer)
        {
            SaveParent(writer);
            writer.Write(YearGroup.StorageIndex);
        }

        public override void Load(BinaryReader reader, Version version, DataContainer container)
        {
            LoadParent(reader, version, container);
            YearGroup = container.YearGroups[reader.ReadInt32()];
        }
    }
}