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
        /// Переопределение конструктора по умолчанию
        /// </summary>
        public Point(int x, int y, Quadrant quadrant = Quadrant.I, int xQuad = 0, int yQuad = 0)
        {
            //TODO: уточнить за такую реализацию
            this.x = x;
            this.y = y;
            this.quadrantId = quadrant;
            this.xQuad = xQuad;
            this.yQuad = yQuad;
            this.quadrantId = GetQuadrant(x, y);
            RecalculateQuadrantCoordinates();
        }

        public int X
        {
            get { return x; }
            set { x = value; }
        }

        public int Y
        {
            get { return y; }
            set { y = value; }
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
            get => default;
            set
            {
            }
        }

        public override bool Equals(object obj)
        {
            return obj is Point point && Equals(point);
        }

        public bool Equals(Point other)
        {
            return x == other.x &&
                   y == other.y &&
                   quadrantId == other.quadrantId &&
                   xQuad == other.xQuad &&
                   yQuad == other.yQuad &&
                   X == other.X &&
                   Y == other.Y &&
                   XQuad == other.XQuad &&
                   YQuad == other.YQuad &&
                   Quadrant == other.Quadrant;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(x);
            hash.Add(y);
            hash.Add(quadrantId);
            hash.Add(xQuad);
            hash.Add(yQuad);
            hash.Add(X);
            hash.Add(Y);
            hash.Add(XQuad);
            hash.Add(YQuad);
            hash.Add(Quadrant);
            return hash.ToHashCode();
        }

        /// <summary>
        /// Получает координаты точки. Определяет квадрант
        /// </summary>
        private Quadrant GetQuadrant(int x, int y)
        {
            if (x < 0 && y > 0)
            {
                return Quadrant.II;
            }
            else if (x < 0 && y < 0)
            {
                return Quadrant.III;
            }
            else if (x > 0 && y < 0)
            {
                return Quadrant.IV;
            }
            else
                return Quadrant.I;
        }

        /// <summary>
        /// По квадранту определяет координаты точек относительно квадранта
        /// </summary>
        private void RecalculateQuadrantCoordinates()
        {
            switch (this.quadrantId)
            {
                case Quadrant.II:
                    this.xQuad = Math.Abs(this.x);
                    this.yQuad = this.y;
                    break;

                case Quadrant.III:
                    this.xQuad = Math.Abs(this.x);
                    this.yQuad = Math.Abs(this.y);
                    break;

                case Quadrant.IV:
                    this.xQuad = this.x;
                    this.yQuad = Math.Abs(this.y);
                    break;

                default:
                    this.xQuad = this.x;
                    this.yQuad = this.y;
                    break;
            }
        }
    }
}