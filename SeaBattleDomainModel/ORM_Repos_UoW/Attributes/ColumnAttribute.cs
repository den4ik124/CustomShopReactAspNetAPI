using ORM_Repos_UoW.Enums;
using System;
using System.Linq;

namespace ORM_Repos_UoW.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        public string ColumnName { get; }
        public ReadWriteOption ReadWriteOption { get; }

        public ColumnAttribute(string columnName = "", ReadWriteOption readWriteOption = ReadWriteOption.Read | ReadWriteOption.Write)
        {
            ColumnName = columnName;
            ReadWriteOption = readWriteOption;
        }
    }
}