using System;
using System.Linq;

namespace ORM_Repos_UoW.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ChildAttribute : Attribute
    {
        public string Table { get; set; }

        public ChildAttribute(string table)
        {
            Table = table;
        }
    }
}