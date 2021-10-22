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
            return ship1.Velocity == ship2.Velocity
                && ship1.GetType() == ship2.GetType()
                && ship1.Size == ship2.Size;
        }

        public static bool operator !=(Ship ship1, Ship ship2)
        {
            return ship1.Velocity != ship2.Velocity
                && ship1.GetType() != ship2.GetType()
                && ship1.Size != ship2.Size;
        }

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
            return $"Velocity: {this.Velocity}\nRange: {this.Range}\nSize: {this.Size}";
        }
    }
}