using System;

namespace OrmRepositoryUnitOfWork.Attributes
{
    public class InheritanceRelationAttribute : Attribute
    {
        public bool IsBaseClass { get; set; } = false;

        public string ColumnMatching { get; set; }
    }
}