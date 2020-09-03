using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace TimetablingWPF
{
    public static class VisualExtensions
    {
        public static void SetPos(this Window window, Vector vector)
        {
            window.Top = vector.Y;
            window.Left = vector.X;
            //System.Windows.Media.GetD
        }

        public static Vector ToVector(this Point point)
        {
            return (Vector)point;
        }

        public static Vector VectorToScreen(this Vector vector)
        {
            PresentationSource source = PresentationSource.FromVisual(Application.Current.MainWindow);
            return vector * source.CompositionTarget.TransformFromDevice;
        }

        public static Vector VectorFromScreen(this Vector vector)
        {
            PresentationSource source = PresentationSource.FromVisual(Application.Current.MainWindow);
            return vector * source.CompositionTarget.TransformToDevice;
        }
        public static Vector PointToScreen(this Point point)
        {
            PresentationSource source = PresentationSource.FromVisual(Application.Current.MainWindow);
            return ((Vector)point) * source.CompositionTarget.TransformFromDevice;
        }

        public static Vector PointFromScreen(this Point point)
        {
            PresentationSource source = PresentationSource.FromVisual(Application.Current.MainWindow);
            return ((Vector)point) * source.CompositionTarget.TransformToDevice;
        }

        public static Vector VectorPos(this Window window)
        {
            return new Vector(window.Left, window.Top);
        }
        public static FrameworkElement FindChild(this FrameworkElement parent, string name)
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                FrameworkElement obj = VisualTreeHelper.GetChild(parent, i) as FrameworkElement;
                if (obj == null)
                {
                    continue;
                }
                if (obj.Name == name)
                {
                    return obj;
                }
                FrameworkElement result = obj.FindChild(name);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }
    }
}
