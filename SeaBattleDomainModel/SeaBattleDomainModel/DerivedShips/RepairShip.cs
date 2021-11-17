using SeaBattleDomainModel.Attributes;
using SeaBattleDomainModel.Entities;
using SeaBattleDomainModel.Interfaces;
using System;

namespace SeaBattleDomainModel.DerivedShips
{
    [Table("Ships")]
    [ShipType(typeID: 2)]
    public class RepairShip : Ship, ICanRepair
    {
        public RepairShip(int id, int velocity, int range, int size) : base(id, velocity, range, size)
        {
        }
        public RepairShip(int velocity, int range, int size) : base(velocity, range, size)
        {
        }

        #region Methods

        #region Methods.Public

        public override void Move()
        {
            throw new NotImplementedException();
        }

        public void Repair()
        {
            throw new NotImplementedException();
        }

        #endregion Methods.Public

        #endregion Methods
    }
}