using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimetablingWPF
{
    public interface IRelationalCollection
    {
        object Parent { get; set; }
        string OtherSetProperty { get; set; }
    };

    public interface IFreezable
    {
        bool Frozen { get; }
        void Freeze();
        void Unfreeze();
    }
    public interface ITab
    {
        MainPage MainPage { get; set; }
        bool Cancel();
    }
}
