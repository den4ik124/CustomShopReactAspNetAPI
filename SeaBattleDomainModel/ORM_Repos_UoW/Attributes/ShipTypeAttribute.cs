using System;
using System.Linq;

namespace ORM_Repos_UoW.Attributes
{
    public class ShipTypeAttribute : Attribute
    {
        public int ShipTypID { get; set; }

        public ShipTypeAttribute(int typeID)
        {
            ShipTypID = typeID;
        }
    }
}