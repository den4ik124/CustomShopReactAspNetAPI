﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleDomainModel.Entities
{
    public abstract class Ship : IEquatable<Ship>
    {
        #region Fields

        private readonly int id;
        private static int nextId = 1;

        #endregion Fields

        #region Constructors

        public Ship()
        {
            id = Ship.nextId++;
        }

        #endregion Constructors

        #region Properties

        public int Id { get { return this.id; } }

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
            return ship1?.Velocity == ship2?.Velocity
                    && ship1?.GetType() == ship2?.GetType()
                    && ship1?.Size == ship2?.Size;
        }

        public static bool operator !=(Ship ship1, Ship ship2)
        {
            return !(ship1 == ship2);
        }

        public abstract void Move();

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
            return HashCode.Combine(this.id);
        }

        public override string ToString()
        {
            return $"Id: {this.id}\nVelocity: {this.Velocity}\nRange: {this.Range}\nSize: {this.Size}";
        }

        #endregion Methods.public

        #endregion Methods
    }
}