using System;
using System.Linq;
using System.Reflection;

namespace ReflectionExtensions
{
    /// <summary>
    /// Clarify, can I use extensions instead of long LINQ queries
    /// </summary>
    public static class TypeExtensions
    {
        public static MethodInfo[] Methods<T>(this T item)
        {
            var type = typeof(T);
            return type.GetMethods();
        }

        public static PropertyInfo[] Properties<T>(this T item)
        {
            var type = typeof(T);
            return type.GetProperties();
        }

        public static PropertyInfo[] Columns<T>(this T item, Type attributeType)
        {
            var type = typeof(T);
            return type.GetProperties().Where(prop => prop.GetCustomAttributes(attributeType).Count() > 0).ToArray();
        }
    }
}