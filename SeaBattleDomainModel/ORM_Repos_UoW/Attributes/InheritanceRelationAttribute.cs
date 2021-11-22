using System;

namespace ORM_Repos_UoW.Attributes
{
    public class InheritanceRelationAttribute : Attribute
    {
        public bool IsBase { get; set; } = false;
    }
}