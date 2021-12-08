using System;

namespace OrmRepositoryUnitOfWork.Attributes
{
    public class InheritanceRelationAttribute : Attribute
    {
        public bool IsBaseClass { get; set; }

        public string? ColumnMatching { get; set; }
    }
}