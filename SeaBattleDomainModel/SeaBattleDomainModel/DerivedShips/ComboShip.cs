using SeaBattleDomainModel.Entities;
using SeaBattleDomainModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleDomainModel.DerivedShips
{
    internal class ComboShip : Ship, ICanShot, ICanRepair
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