using ORM_Repos_UoW.Attributes;
using SeaBattleDomainModel.Entities;
using SeaBattleDomainModel.Interfaces;
using System;

namespace SeaBattleDomainModel.DerivedShips
{
    [Table("Ships")]
    [InheritanceRelation]
    [ShipType(ShipTypeID = 3, ShipType = typeof(BattleShip), BaseType = typeof(Ship), ColumnMatching = "TypeId")]
    public class ComboShip : Ship, ICanShot, ICanRepair
    {
        /// <summary>
        /// for custom ORM
        /// </summary>
        public ComboShip()
        {
        }

        public ComboShip(int velocity, int range, int size) : base(velocity, range, size)
        {
        }

        #region Methods

        #region Methods.Public

        public void MakeShot()
        {
            throw new NotImplementedException();
        }

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