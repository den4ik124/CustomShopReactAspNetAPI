using ORM_Repos_UoW.Attributes;
using System;

namespace SeaBattleDomainModel.Entities
{
    [Table("Cells")]
    public class Cell
    {
        #region Fields

        private int? shipId;
        private int pointId;

        #endregion Fields

        [Column("Id")]
        public int Id { get; set; }

        [Column("BattleFieldID")]
        public int BattleFieldId { get; set; }

        [Column("ShipID")]
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

        [Column("PointID")]
        public int PointId
        {
            get => this.pointId;
            set => this.pointId = value;
        }

        #region Constructors

        /// <summary>
        /// Для корректной работы ORM
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

        public Ship Ship { get; set; }
        public Point Point { get; set; }
        public double DistanceToOrigin { get; }

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
            //return $"\tId:\n{Id}\n" +
            //       $"\tShip:\n{Ship}\n" +
            //       $"\tPoint:\n{Point}";
            return $"Id:\t{Id}\n" +
                   $"Ship:\t{ShipId}\n" +
                   $"Point:\t{PointId}";
        }

        #endregion Methods.public

        #endregion Methods
    }
}