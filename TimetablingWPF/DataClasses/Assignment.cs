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

namespace TimetablingWPF
{
    /// <summary>
    /// Represents an assignment between a lesson and a teacher
    /// </summary>
    public class Assignment
    {
        public Teacher Teacher { get; }
        public Lesson Lesson { get; }
        public int LessonCount { get; }

        /// <summary>
        /// The string representation of this object from a teacher's perspective
        /// </summary>
        public string TeacherString { get; }
        /// <summary>
        /// The string representation of this object from a lesson's perspective
        /// </summary>
        public string LessonString { get; }
        /// <summary>
        /// </summary>
        /// <param name="teacher"></param>
        /// <param name="periods"></param>
        public Assignment(Teacher teacher, Lesson lesson, int periods)
        {
            Teacher = teacher;
            Lesson = lesson;
            LessonCount = periods;
            LessonString = $"{Teacher} ({LessonCount})";
            TeacherString = $"{Lesson} ({LessonCount})";
        }
        /// <summary>
        /// Creates references to this assignment in the teacher and lesson assignments list
        /// </summary>
        public override string ToString()
        {
            return $"{Teacher} - {Lesson} ({LessonCount})";
        }
        public int TotalPeriods => LessonCount * Lesson.LessonLength;
    }
}