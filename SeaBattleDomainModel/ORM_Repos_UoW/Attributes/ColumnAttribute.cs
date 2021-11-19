using ORM_Repos_UoW.Enums;
using System;

namespace ORM_Repos_UoW.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        public string ColumnName { get; }
        public ReadWriteOption ReadWriteOption { get; }
        public KeyType KeyType { get; }

        public ColumnAttribute(string columnName = "", /*KeyType keyType = KeyType.None,*/ ReadWriteOption readWriteOption = ReadWriteOption.Read | ReadWriteOption.Write)
        {
            ColumnName = columnName;
            ReadWriteOption = readWriteOption;
            //KeyType = keyType;
        }
    }
}