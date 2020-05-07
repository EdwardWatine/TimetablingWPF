using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.VisualStyles;

namespace TimetablingWPF.Searching
{
    public abstract class SearchBase
    {
        protected string Optiontype { get; set; }
        protected string Verb { get; set; }
        public abstract UIElement GenerateUI();
        public abstract bool Search(object item);
    }
}
