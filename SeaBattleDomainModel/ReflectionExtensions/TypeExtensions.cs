using System.Reflection;

namespace ReflectionExtensions
{
    public static class TypeExtensions
    {
        public static MethodInfo[] GetMethods<T>(this T item)
        {
            var type = typeof(T);
            return type.GetMethods();
        }
        public static PropertyInfo[] GetProperties<T>(this T item)
        {
            var type = typeof(T);
            return type.GetProperties();
        }
    }
}