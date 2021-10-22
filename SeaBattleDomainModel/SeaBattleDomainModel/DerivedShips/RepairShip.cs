using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleDomainModel.DerivedShips
{
    internal class RepairShip : Ship, ICanRepair
    {
        public override Point[] Points { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override int Velocity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override int Range { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override int Size { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Move()
        {
            throw new NotImplementedException();
        }

        public void Repair(Ship target)
        {
            throw new NotImplementedException();
        }
    }
}