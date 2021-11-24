using System;

namespace ORM_Repos_UoW.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class RelatedEntityAttribute : Attribute
    {
        public string Table { get; set; }
        public Type RelatedType { get; set; }

        public bool IsCollection { get; set; }

        public RelatedEntityAttribute()
        {
        }

        //public ChildAttribute(string table, Type relatedType)
        //{
        //    Table = table;
        //    RelatedType = relatedType;
        //}
    }
}