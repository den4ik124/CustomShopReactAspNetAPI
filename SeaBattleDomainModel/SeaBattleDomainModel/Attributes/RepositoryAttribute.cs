using System;

namespace SeaBattleDomainModel.Entities
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct )]
    public class RepositoryAttribute : Attribute
    {
        public RepositoryAttribute(string tableName = "")
        {
            TableName = tableName;
        }

        public string TableName { get; }
    }
}