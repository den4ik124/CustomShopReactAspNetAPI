using System;

namespace ORM_Repos_UoW.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class TableAttribute : Attribute
    {
        public TableAttribute(string tableName = "")
        {
            TableName = tableName;
        }

        public string TableName { get; }
    }
}