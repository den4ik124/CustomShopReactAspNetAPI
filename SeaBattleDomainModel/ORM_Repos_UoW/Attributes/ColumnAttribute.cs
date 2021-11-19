using System;
using System.Linq;

namespace ORM_Repos_UoW.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        public string ColumnName { get; }

        public ColumnAttribute(string columnName = "")
        {
            ColumnName = columnName;
        }
    }
}