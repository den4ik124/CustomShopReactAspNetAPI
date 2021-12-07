using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ReflectionExtensions
{
    public enum PropertyKeyType
    {
        Primary,
        Foreign,
        None,
        All,
    }

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

        public static IEnumerable<PropertyInfo> Columns<T>(this T item, Type attributeType)
        {
            var type = typeof(T);
            return type.GetProperties().Where(prop => prop.GetCustomAttributes(attributeType).Any());
        }

        public static IEnumerable<PropertyInfo> Columns(this Type type, Type attributeType)
        {
            return type.GetProperties().Where(prop => prop.GetCustomAttributes(attributeType).Any());
        }

        public static IEnumerable<Type> GetTypes(this Assembly assembly, Type attributeType)
        {
            return assembly.GetTypes().Where(prop => prop.GetCustomAttributes(attributeType).Any());
        }

        public static Assembly GetAssembly(Type attributeType)
        {
            return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => assembly.GetCustomAttributes(attributeType).Any());
        }
    }
}