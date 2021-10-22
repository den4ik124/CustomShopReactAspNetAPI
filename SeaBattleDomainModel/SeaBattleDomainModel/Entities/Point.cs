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
        //public Point(int x, int y)
        //{
        //    //TODO: проиниализировать все поля
        //}

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

        public int XQuad { get; set; }
        public int YQuad { get; set; }

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
            return default;
        }

        /// <summary>
        /// По квадранту определяет координаты точек относительно квадранта
        /// </summary>
        private void RecalculateQuadrantCoordinates()
        {
            throw new System.NotImplementedException();
        }
    }
}