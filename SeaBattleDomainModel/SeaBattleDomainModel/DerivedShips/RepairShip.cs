﻿using ORM_Repos_UoW.Attributes;
using SeaBattleDomainModel.Entities;
using SeaBattleDomainModel.Interfaces;
using System;

namespace SeaBattleDomainModel.DerivedShips
{
    [Table("Ships")]
    [InheritanceRelation(ColumnMatching = "TypeId")]
    [Type(TypeID = 2, Type = typeof(BattleShip), BaseType = typeof(Ship), ColumnMatching = "TypeId")]
    public class RepairShip : Ship, ICanRepair
    {
        /// <summary>
        /// for custom ORM
        /// </summary>
        public RepairShip()
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