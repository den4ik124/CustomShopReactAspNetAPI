using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleDomainModel.Entities
{
    public abstract class Ship : IEquatable<Ship>
    {
        #region Fields

        //fields

        #endregion Fields

        #region Constructors

        //ctors

        #endregion Constructors

        #region Properties

        public abstract int Velocity { get; set; }

        public abstract int Range { get; set; }

        public abstract int Size { get; set; }

        #endregion Properties

        #region Methods

        #region Methods.Private

        //private

        #endregion Methods.Private

        #region Methods.public

        public static bool operator ==(Ship ship1, Ship ship2)
        {
            //TODO (DONE) : Possible nullReferenceException. https://gitlab.nixdev.co/net-projects/education/dboryhin/-/merge_requests/1#note_404485
            return ship1 != null && ship2 != null
                    && ship1.Velocity == ship2.Velocity
                    && ship1.GetType() == ship2.GetType()
                    && ship1.Size == ship2.Size;
        }

        public static bool operator !=(Ship ship1, Ship ship2)
        {
            //TODO (DONE) : simplify logic (use what already exist)
            return !(ship1 == ship2);
        }

        public abstract void Move();

        public override bool Equals(object obj)
        {
            //TODO (DONE) : please handle case when obj is not a Ship
            if (obj is not Ship)
            {
                return false;
            }
            return Equals(obj as Ship);
        }

        public bool Equals(Ship other)
        {
            // TODO (DONE): Please check the most recent coding guideline whether we should use this.Velocity ... ? https://gitlab.nixdev.co/net-projects/education/dboryhin/-/merge_requests/1#note_404491
            return other != null &&
                   this.Velocity == other.Velocity &&
                   this.Range == other.Range &&
                   this.Size == other.Size;
        }

        public override int GetHashCode()
        {
            //TODO : Определить какие параметры указать для хэша. Должны быть readonly
            return HashCode.Combine(this.Velocity, this.Range, this.Size);
        }

        public override string ToString()
        {
            return $"Velocity: {this.Velocity}\nRange: {this.Range}\nSize: {this.Size}";
        }

        #endregion Methods.public

        #endregion Methods
    }
}