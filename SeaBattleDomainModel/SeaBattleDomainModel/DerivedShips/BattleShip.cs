using SeaBattleDomainModel.Entities;
using SeaBattleDomainModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleDomainModel.DerivedShips
{
    public class BattleShip : Ship, ICanShot
    {
        #region Fields

        //fields

        #endregion Fields

        #region Constructors

        //ctors

        #endregion Constructors

        #region Properties

        //public override int Velocity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        //public override int Range { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        //public override int Size { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        #region Code for tests

        public override int Velocity { get; set; }

        public override int Range { get; set; }
        public override int Size { get; set; }

        #endregion Code for tests

        #endregion Properties

        #region Methods

        #region Methods.Private

        //private

        #endregion Methods.Private

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