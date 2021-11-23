using ORM_Repos_UoW.Attributes;
using ORM_Repos_UoW.Enums;
using System;

namespace SeaBattleDomainModel.Entities
{
    [Table("Points")]
    public struct Point : IEquatable<Point>
    {
        #region Fields

        private int x; //TODO: могу ли я поменять readonly на свойство {get; private set;} ?

        private int y;

        private Quadrant quadrantId;

        private int xAbsolute;

        private int yAbsolute;

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

        [Column("PointsId", ReadWriteOption.Write)]
        public int Id { get; set; }

        [Column("X")]
        public int X
        {
            get { return this.x; }
            private set
            {
                this.x = value;
                this.quadrantId = GetQuadrant(this.x, this.y);
                this.xAbsolute = Math.Abs(this.x);
            }
        }

        [Column("Y")]
        public int Y
        {
            get { return this.y; }
            private set
            {
                this.y = value;
                this.quadrantId = GetQuadrant(this.x, this.y);
                this.yAbsolute = Math.Abs(this.y);
            }
        }

        public int XAbsolute
        {
            get { return this.xAbsolute; }
            private set { this.xAbsolute = value; }
        }

        public int YAbsolute
        {
            get { return this.yAbsolute; }
            private set { this.yAbsolute = value; }
        }

        public Quadrant Quadrant
        {
            get { return this.quadrantId; }
            private set { this.quadrantId = value; }
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