using SeaBattleDomainModel.Entities;
using SeaBattleDomainModel.Interfaces;
using System;

namespace SeaBattleDomainModel.DerivedShips
{
    public class ComboShip : Ship, ICanShot, ICanRepair
    {
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