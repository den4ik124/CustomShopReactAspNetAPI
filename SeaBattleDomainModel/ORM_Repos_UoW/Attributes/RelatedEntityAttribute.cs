using System;

namespace OrmRepositoryUnitOfWork.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class RelatedEntityAttribute : Attribute
    {
        public string? Table { get; set; }
        public Type? RelatedType { get; set; }

        public bool IsCollection { get; set; }
    }
}