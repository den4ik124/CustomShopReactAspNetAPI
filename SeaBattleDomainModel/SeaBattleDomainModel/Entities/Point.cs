using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleDomainModel.Entities
{
    public struct Point : IEquatable<Point>
    {
        #region Fields

        private int x;

        private int y;

        private Quadrant quadrantId;

        private int xQuad;

        private int yQuad;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Overriding the default constructor
        /// </summary>
        public Point(int x, int y) : this()
        {
            this.x = x;
            this.y = y;
            this.quadrantId = GetQuadrant(x, y);
            this.xQuad = Math.Abs(x);
            this.yQuad = Math.Abs(y);
        }

        #endregion Constructors

        #region Properties

        public int X
        {
            get { return this.x; }
            set { this.x = value; }
        }

        public int Y
        {
            get { return this.y; }
            set { this.y = value; }
        }

        public int XQuad
        {
            get { return this.xQuad; }
            set { this.xQuad = value; }
        }

        public int YQuad
        {
            get { return this.yQuad; }
            set { this.yQuad = value; }
        }

        public Quadrant Quadrant
        {
            get => this.quadrantId;
        }

        #endregion Properties

        #region Methods

        #region Methods.Private

        /// <summary>
        /// Gets the coordinates of the point. Defines a quadrant
        /// </summary>
        private Quadrant GetQuadrant(int x, int y)
        {
            switch ((x, y))
            {
                case ( > 0, > 0):
                    return Quadrant.First;

                case ( < 0, > 0):
                    return Quadrant.Second;

                case ( < 0, < 0):
                    return Quadrant.Third;

                case ( > 0, < 0):
                    return Quadrant.Fourth;

                case (0, > 0):
                    return Quadrant.First | Quadrant.Second;

                case (0, < 0):
                    return Quadrant.Third | Quadrant.Fourth;

                case ( > 0, 0):
                    return Quadrant.First | Quadrant.Fourth;

                case ( < 0, 0):
                    return Quadrant.Second | Quadrant.Third;

                default:
                    return Quadrant.First | Quadrant.Second | Quadrant.Third | Quadrant.Fourth;
            }
        }

        #endregion Methods.Private

        #region Methods.public

        public override bool Equals(object obj)
        {
            return obj is Point point && Equals(point);
        }

        public bool Equals(Point other)
        {
            return this.x == other.x &&
                   this.y == other.y;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(this.x);
            hash.Add(this.y);
            return hash.ToHashCode();
        }

        #endregion Methods.public

        #endregion Methods
    }
}