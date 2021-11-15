using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleDomainModel.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        public string ColumnName { get; }
        public ColumnAttribute(string columnName = "")
        {
            ColumnName = columnName;
        }
    }
}
