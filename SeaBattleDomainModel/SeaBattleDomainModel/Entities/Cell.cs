using System;

namespace SeaBattleDomainModel.Entities
{
    [Repository("Cells")]
    public class Cell
    {
        #region Constructors

        public Cell(Point point)
        {
            Point = point;
            DistanceToOrigin = GetDistanceToOrigin(point);
        }

        public Cell(Ship ship, Point point) : this(point)
        {
            Ship = ship;
        }

        #endregion Constructors

        #region Properties

        public Ship Ship { get; set; }

        public Point Point { get; set; }
        public double DistanceToOrigin { get; set; }

        #endregion Properties

        #region Methods

        #region Methods.Private

        /// <summary>
        /// Calculates distance from point to origin
        /// </summary>
        /// <param name="point">Current point</param>
        /// <returns>Distance value</returns>
        private double GetDistanceToOrigin(Point point)
        {
            return Math.Sqrt(point.X * point.X + point.Y * point.Y);
        }

        #endregion Methods.Private

        #region Methods.public

        public override string ToString()
        {
            return $"\tShip:\n{Ship}\n" +
                   $"\tPoint:\n{Point}";
        }

        #endregion Methods.public

        #endregion Methods
    }
}