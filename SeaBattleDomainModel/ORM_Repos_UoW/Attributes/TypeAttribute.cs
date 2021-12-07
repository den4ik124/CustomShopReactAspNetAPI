using System;
using System.Linq;

namespace OrmRepositoryUnitOfWork.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class TypeAttribute : Attribute
    {
        private Type? baseType;

        public int TypeID { get; set; }

        public Type? BaseType { get => this.baseType; set => this.baseType = value; }

        public string? ColumnMatching { get; set; }
    }
}