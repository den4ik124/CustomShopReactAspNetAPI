using System;

namespace ORM_Repos_UoW.Attributes
{
    public class InheritanceRelationAttribute : Attribute
    {
        public bool IsBaseClass { get; set; } = false;

        public string ColumnMatching { get; set; }
    }
}