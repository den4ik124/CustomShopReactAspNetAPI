using SeaBattleDomainModel.Attributes;
using System;

namespace SeaBattleDomainModel.Entities
{
    [Repository("Points")]

    public struct Point : IEquatable<Point>
    {
        #region Fields
        [Column("X")]
        private readonly int x;

        [Column("Y")]
        private readonly int y;

        private readonly Quadrant quadrantId;

        private readonly int xAbsolute;

        private readonly int yAbsolute;

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
            this.xAbsolute = Math.Abs(x);
            this.yAbsolute = Math.Abs(y);
        }

        #endregion Constructors

        #region Properties

        public int X
        {
            get { return this.x; }
        }

        public int Y
        {
            get { return this.y; }
        }

        public int XAbsolute
        {
            get { return this.xAbsolute; }
        }

        public int YAbsolute
        {
            get { return this.yAbsolute; }
        }

        public Quadrant Quadrant
        {
            get { return this.quadrantId; }
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