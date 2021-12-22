using System;

namespace SeaBattleDomainModel.Entities
{
    public abstract class Ship : IEquatable<Ship>
    {
        #region Constructors

        /// <summary>
        /// Для корректной работы ORM
        /// </summary>
        public Ship()
        {
        }

        public Ship(int velocity, int range, int size)
        {
            this.Velocity = velocity;
            this.Range = range;
            this.Size = size;
        }

        #endregion Constructors

        #region Properties

        public int Id { get; set; }

        public int Velocity { get; set; }

        public int Size { get; set; }

        public int Range { get; set; }

        #endregion Properties

        #region Methods

        #region Methods.public

        public static bool operator ==(Ship ship1, Ship ship2)
        {
            return ship1?.Velocity == ship2?.Velocity
                    && ship1?.GetType() == ship2?.GetType()
                    && ship1?.Size == ship2?.Size;
        }

        public static bool operator !=(Ship ship1, Ship ship2)
        {
            return !(ship1 == ship2);
        }

        public virtual void Move()
        {
        }

        public override bool Equals(object obj)
        {
            if (obj is not Ship)
            {
                return false;
            }
            return Equals(obj as Ship);
        }

        public bool Equals(Ship other)
        {
            return other != null &&
                   this.Velocity == other.Velocity &&
                   this.Range == other.Range &&
                   this.Size == other.Size;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Id);
        }

        public override string ToString()
        {
            return $"Id: {this.Id}\nVelocity: {this.Velocity}\nRange: {this.Range}\nSize: {this.Size}";
        }

        #endregion Methods.public

        #endregion Methods
    }
}