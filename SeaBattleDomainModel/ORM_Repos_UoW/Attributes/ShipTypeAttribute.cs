using System;
using System.Linq;

namespace ORM_Repos_UoW.Attributes
{
    public class ShipTypeAttribute : Attribute
    {
        private Type baseType;

        public int ShipTypeID { get; set; }
        public Type ShipType { get; set; }
        public Type BaseType { get => baseType; set => baseType = value; }
        public string BaseTypeName { get => this.baseType.Name; }

        public ShipTypeAttribute()
        {
        }
    }
}