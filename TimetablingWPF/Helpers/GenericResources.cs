using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using static TimetablingWPF.GenericHelpers;

namespace TimetablingWPF
{
    public static class GenericResources
    {
        public static SolidColorBrush BLUE { get; } = FromRGBA(0, 0, 255);
        public static SolidColorBrush RED { get; } = FromRGBA(255, 0, 0);
        public static SolidColorBrush GREEN { get; } = FromRGBA(0, 255, 0);
        public static SolidColorBrush WHITE { get; } = FromRGBA(255, 255, 255);
        public static SolidColorBrush BLACK { get; } = FromRGBA(0, 0, 0);
        public static SolidColorBrush TRANSPARENT { get; } = FromRGBA(0, 0, 0, 0);
        public static SolidColorBrush A8GRAY { get; } = FromRGBA(168, 168, 168);
        static GenericResources()
        {
            BLUE.Freeze();
            RED.Freeze();
            GREEN.Freeze();
            WHITE.Freeze();
            BLACK.Freeze();
            TRANSPARENT.Freeze();
            A8GRAY.Freeze();
        }
    }
}
