using ORM_Repos_UoW.Enums;
using System;

namespace ORM_Repos_UoW.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        public string ColumnName { get; set; }
        public ReadWriteOption ReadWriteOption { get; set; }
        public KeyType KeyType { get; set; } = KeyType.None;

        public Type BaseType { get; set; } = typeof(object);

        public ColumnAttribute(string columnName = "", KeyType keyType = KeyType.None, ReadWriteOption readWriteOption = ReadWriteOption.Read | ReadWriteOption.Write)
        {
            ColumnName = columnName;
            ReadWriteOption = readWriteOption;
            //KeyType = keyType;
        }
    }
}