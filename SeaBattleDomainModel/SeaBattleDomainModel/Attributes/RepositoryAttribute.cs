using System;

namespace SeaBattleDomainModel.Entities
{
    public class RepositoryAttribute : Attribute
    {
        public RepositoryAttribute(string tableName = "")
        {
            TableName = tableName;
        }

        public string TableName { get; }
    }
}