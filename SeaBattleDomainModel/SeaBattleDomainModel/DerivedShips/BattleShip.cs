using SeaBattleDomainModel.Entities;
using SeaBattleDomainModel.Interfaces;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeaBattleDomainModel.DerivedShips
{
    [Table("Ships")]
    public class BattleShip : Ship, ICanShot
    {
        /// <summary>
        /// for custom ORM
        /// </summary>
        public BattleShip()
        {
        }

        public BattleShip(int velocity, int range, int size) : base(velocity, range, size)
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

        #endregion Methods.Public

        #endregion Methods
    }
}