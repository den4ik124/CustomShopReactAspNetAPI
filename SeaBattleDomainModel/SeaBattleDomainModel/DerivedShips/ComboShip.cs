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
        public override int Velocity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override int Range { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override int Size { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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
    }
}