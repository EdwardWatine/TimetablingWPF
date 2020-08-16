using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ObservableComputations;

namespace TimetablingWPF
{
    class IndependentSet : IEnumerable<Form>, ISaveable
    {
        public IndependentSet(Subject link)
        {
            LinkedSubject = link;
            //Forms.AddRange();
        }
        public ObservableCollectionExtended<Form> Forms { get; } = new ObservableCollectionExtended<Form>();
        public Subject LinkedSubject { get; }
        public IEnumerator<Form> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void Load(BinaryReader reader, Version version, DataContainer container)
        {
            throw new NotImplementedException();
        }

        public void Save(BinaryWriter writer)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
