using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimetablingWPF
{
    public static class BackgroundTaskManager
    {
        public static ObservableCollection<BackgroundTask> Tasks { get; } = new ObservableCollection<BackgroundTask>();
    }

    public struct BackgroundTask : IEquatable<BackgroundTask>
    {
        public string Name { get; }
        public string Description { get; }
        public BackgroundTask(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public override bool Equals(object obj)
        {
            return obj is BackgroundTask task && Equals(task);
        }

        public override int GetHashCode()
        {
            return 17 * Name.GetHashCode() + Description.GetHashCode();
        }

        public static bool operator ==(BackgroundTask left, BackgroundTask right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BackgroundTask left, BackgroundTask right)
        {
            return !(left == right);
        }

        public bool Equals(BackgroundTask task)
        {
            return task.Name == Name && task.Description == Description;
        }
    }
}
