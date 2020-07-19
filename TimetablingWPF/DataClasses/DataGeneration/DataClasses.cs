using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimetablingWPF
{
    public static partial class DataGeneration
    {
        public static void GenerateSingleDataType<T>(int number) where T : BaseDataClass, new()
        {
            for (int i = 0; i < number; i++)
            {
                new T()
                {
                    Name = $"{typeof(T).Name}-{i}",
                    Shorthand = $"{typeof(T).Name.Substring(0, 2).ToUpperInvariant()}{i}"
                }.Commit();
            }
        }
        public static void GenerateAllDataTypes(int number)
        {
            foreach (Type type in DataHelpers.UserTypes) {
                for (int i = 0; i < number; i++)
                {
                    BaseDataClass obj = (BaseDataClass)Activator.CreateInstance(type);
                    obj.Name = $"{type.Name}-{i}";
                    obj.Shorthand = $"{type.Name.Substring(0, 2).ToUpperInvariant()}{i}";
                    obj.Commit();
                }
            }
        }
    }
}
