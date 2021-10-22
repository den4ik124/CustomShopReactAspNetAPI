using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleDomainModel.Entities
{
    public abstract class Ship : IEquatable<Ship>
    {
        public abstract int Velocity { get; set; }

        public abstract int Range { get; set; }

        public abstract int Size { get; set; }

        public static bool operator ==(Ship ship1, Ship ship2)
        {
            //TODO: реализовать перегрузку оператора сравнения
            return ship1.Velocity == ship2.Velocity;
        }

        public static bool operator !=(Ship ship1, Ship ship2)
        {
            //TODO: реализовать перегрузку оператора сравнения
            return ship1.Velocity != ship2.Velocity;
        }

        //public void FillShipPoints(Point head, Point tail)
        //{
        //    if (head.Equals(tail))
        //    {
        //        Points = new Point[1] { head }; //имеем корабль-точку
        //    }
        //}

        public abstract void Move();

        public override bool Equals(object obj)
        {
            return Equals(obj as Ship);
        }

        public bool Equals(Ship other)
        {
            return other != null &&
                   Velocity == other.Velocity &&
                   Range == other.Range &&
                   Size == other.Size;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Velocity, Range);
        }

        public override string ToString()
        {
            //TODO: реализовать переопределние для вывода состояния в строку
            return base.ToString();
        }
    }
}