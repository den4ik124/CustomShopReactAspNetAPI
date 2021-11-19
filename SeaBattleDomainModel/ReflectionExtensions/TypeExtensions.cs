using System.Reflection;

namespace ReflectionExtensions
{
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
    }
}