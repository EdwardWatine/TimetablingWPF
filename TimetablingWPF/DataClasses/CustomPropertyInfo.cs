using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

namespace TimetablingWPF
{
    public class CustomPropertyInfo
    {
        public CustomPropertyInfo(Type type, string property, string alias = null, Func<object, string> display = null)
        {
            PropertyInfo = type.GetProperty(property);
            Alias = alias ?? property;
            Display = display ?? new Func<object, string>(o => Alias);
        }
        public PropertyInfo PropertyInfo { get; }
        public string Alias { get; set; }
        public Func<object, string> Display { get; set; }
    }
}
