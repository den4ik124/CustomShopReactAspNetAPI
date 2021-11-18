using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleDomainModel.Attributes
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
