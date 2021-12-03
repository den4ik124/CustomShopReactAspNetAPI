using System;

namespace OrmRepositoryUnitOfWork.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class TableAttribute : Attribute
    {
        public string TableName { get; set; }

        public bool IsRelatedTable { get; set; }
        public bool IsStaticDataTable { get; set; }

        public TableAttribute(string tableName, bool isRelated = false)
        {
            TableName = tableName;
            IsRelatedTable = isRelated;
        }
    }
}