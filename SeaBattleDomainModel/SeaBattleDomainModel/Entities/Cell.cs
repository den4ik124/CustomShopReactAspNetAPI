using System;

namespace SeaBattleDomainModel.Entities
{
    public class Cell
    {
        #region Fields

        private int? shipId;
        private int pointId;

        private Point point;
        private Ship ship;

        #endregion Fields

        public int Id { get; set; }

        public int BattleFieldId { get; set; }

        public int? ShipId
        {
            get
            {
                if (this.Ship != null)
                    return this.Ship.Id;
                else
                    return this.shipId;
            }
            set => this.shipId = value;
        }

        public int PointId
        {
            get => this.pointId;
            set => this.pointId = value;
        }

        #region Constructors

        /// <summary>
        /// for ORM
        /// </summary>
        public Cell()
        {
        }

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

        public Ship Ship
        {
            get => this.ship;
            set
            {
                this.ship = value;
                if (this.ship != null)
                {
                    this.shipId = this.ship.Id;
                }
            }
        }

        public Point Point
        {
            get => this.point;
            set
            {
                this.point = value;
                this.pointId = this.point.Id;
                DistanceToOrigin = GetDistanceToOrigin(this.point);
            }
        }

        public double DistanceToOrigin { get; private set; }

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
            return $"\tId:\n{Id}\n" +
                   $"\tShip:\n{Ship}\n" +
                   $"\tPoint:\n{Point}";
        }

        #endregion Methods.public

        #endregion Methods
    }
}