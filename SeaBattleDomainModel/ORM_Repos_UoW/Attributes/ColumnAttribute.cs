using OrmRepositoryUnitOfWork.Enums;
using System;

namespace OrmRepositoryUnitOfWork.Attributes
{
    /// <summary>
    /// Mark property to match the data with corresponding column in table
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        public string ColumnName { get; set; }
        public ReadWriteOption ReadWriteOption { get; set; }
        public KeyType KeyType { get; set; } = KeyType.None;

        public Type BaseType { get; set; } = typeof(object);

        public bool AllowNull { get; set; } = false;

        public ColumnAttribute(string columnName = "", KeyType keyType = KeyType.None, ReadWriteOption readWriteOption = ReadWriteOption.Read | ReadWriteOption.Write)
        {
            ColumnName = columnName;
            ReadWriteOption = readWriteOption;
        }
    }
}