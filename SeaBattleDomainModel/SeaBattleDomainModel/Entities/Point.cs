using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleDomainModel.Entities
{
    public struct Point : IEquatable<Point>
    {
        private int x;

        private int y;

        private Quadrant quadrantId;

        private int xQuad;

        private int yQuad;

        /// <summary>
        /// Overriding the default constructor
        /// </summary>
        public Point(int x, int y)
        {
            //TODO: уточнить за такую реализацию
            this.x = x;
            this.y = y;
            this.quadrantId = default;
            this.xQuad = default;
            this.yQuad = default;
            this.quadrantId = GetQuadrant(x, y);
            RecalculateQuadrantCoordinates();
        }

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

        public override bool Equals(object obj)
        {
            return obj is Point point && Equals(point);
        }

        public bool Equals(Point other)
        {
            return this.x == other.x &&
                   this.y == other.y &&
                   this.quadrantId == other.quadrantId &&
                   this.xQuad == other.xQuad &&
                   this.yQuad == other.yQuad &&
                   this.X == other.X &&
                   this.Y == other.Y &&
                   this.XQuad == other.XQuad &&
                   this.YQuad == other.YQuad &&
                   this.Quadrant == other.Quadrant;
        }

        public override int GetHashCode()
        {
            //TODO (DONE): Please simplify hashcode. What makes point unique?
            HashCode hash = new HashCode();
            hash.Add(this.x);
            hash.Add(this.y);
            return hash.ToHashCode();
        }

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

        /// <summary>
        /// By quadrant determines the coordinates of points relative to the quadrant
        /// </summary>
        private void RecalculateQuadrantCoordinates()
        {
            switch (this.quadrantId)
            {
                case Quadrant.Second:
                    this.xQuad = Math.Abs(this.x);
                    this.yQuad = this.y;
                    break;

                case Quadrant.Third:
                    this.xQuad = Math.Abs(this.x);
                    this.yQuad = Math.Abs(this.y);
                    break;

                case Quadrant.Fourth:
                    this.xQuad = this.x;
                    this.yQuad = Math.Abs(this.y);
                    break;

                case Quadrant.First | Quadrant.Second:
                    this.xQuad = Math.Abs(this.x);
                    this.yQuad = this.y;
                    break;

                case Quadrant.Second | Quadrant.Third:
                    this.xQuad = Math.Abs(this.x);
                    this.yQuad = Math.Abs(this.y);
                    break;

                case Quadrant.Third | Quadrant.Fourth:
                    this.xQuad = Math.Abs(this.x);
                    this.yQuad = Math.Abs(this.y);
                    break;

                case Quadrant.First | Quadrant.Fourth:
                    this.xQuad = Math.Abs(this.x);
                    this.yQuad = this.y;
                    break;

                default:
                    this.xQuad = this.x;
                    this.yQuad = this.y;
                    break;
            }
        }
    }
}