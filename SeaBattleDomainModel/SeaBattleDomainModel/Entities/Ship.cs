using SeaBattleDomainModel.Attributes;
using System;

namespace SeaBattleDomainModel.Entities
{
    [Table("Ships")]
    public abstract class Ship : IEquatable<Ship>
    {
        #region Fields

        private static int nextId = 1;

        #endregion Fields

        #region Constructors

        public Ship()
        {
        }

        public Ship(int id, int velocity, int range, int size)
        {
            this.Id = id;
            this.Velocity = velocity;
            this.Range = range;
            this.Size = size;
        }

        public Ship(int velocity, int range, int size)
        {
            Id = Ship.nextId++;
            this.Velocity = velocity;
            this.Range = range;
            this.Size = size;
        }

        #endregion Constructors

        #region Properties

        [Column("Id")]
        public int Id { get; set; }

        [Column("Velocity")]
        public int Velocity { get; set; }
        
        [Column("Size")]
        public int Size { get; set; }

        [Column("Range")]
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