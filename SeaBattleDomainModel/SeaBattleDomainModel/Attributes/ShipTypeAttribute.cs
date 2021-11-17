using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleDomainModel.Attributes
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
