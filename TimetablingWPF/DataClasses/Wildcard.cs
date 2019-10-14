using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimetablingWPF
{
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    class Wildcard
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        public override bool Equals(object obj)
        {
            return obj.GetType() == typeof(BaseDataClass) || obj.GetType().BaseType == typeof(BaseDataClass);
        }
        public override string ToString()
        {
            return "Any";
        }
    }
}
