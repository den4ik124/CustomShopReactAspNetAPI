using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SeaBattleDomainModel.Entities
{
    public class BattleField
    {
        #region Fields

        private const int coefficient = 2;
        private const int expressionPower = 2;
        private const int battleFieldPointsIncreaser = 1;

        private readonly List<Ship> ships;

        private readonly int battleFieldSideLength;

        private Dictionary<Point, Cell> cells;

        #endregion Fields

        #region Constructors

        public BattleField(int fieldSideLength)
        {
            this.battleFieldSideLength = fieldSideLength + battleFieldPointsIncreaser;
            this.ships = new List<Ship>();
            this.cells = new Dictionary<Point, Cell>(this.battleFieldSideLength * this.battleFieldSideLength);
            FillCells();
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Initialize new battle field
        /// </summary>
        /// <param name="halfSideLength">The side of separate quadrant</param>
        private void FillCells()
        {
            int battleFieldHalfSide = battleFieldSideLength / coefficient;
            for (int y = battleFieldHalfSide; y >= -battleFieldHalfSide; y--)
            {
                for (int x = -battleFieldHalfSide; x <= battleFieldHalfSide; x++)
                {
                    var point = new Point(x, y);
                    cells.Add(point, new Cell(point));
                }
            }
        }

        public Ship this[Quadrant quadrant, int x, int y]
        {
            get
            {
                return GetShipByCoordinates(quadrant, x, y);
            }
        }

        /// <summary>
        /// Finds a ship in the list of ships
        /// </summary>
        /// <returns>Ship from list</returns>
        private Ship GetShipByCoordinates(Quadrant quadrant, int x, int y)
        {
            return cells.Where(point => point.Key.XAbsolute == x
                                     && point.Key.YAbsolute == y
                                     && point.Key.Quadrant == quadrant)
                        .FirstOrDefault().Value.Ship; ;
        }

        public void AddShip(Ship ship, Point head, Point tail)
        {
            if (!IsLocationValid(head, tail))
            {
                return;
            }
            ship.Size = GetShipSize(head, tail);

            var pointsBetween = GetPointsBetween(head, tail); //get points between head and tail

            foreach (var point in pointsBetween)
            {
                var cell = GetCellByPoint(point); //get a cell for each point between head and tail
                cell.Ship = ship; //in the resulting cell we transfer the ship, which will be located between the head and tail
            }
            this.ships.Add(ship);
        }

        /// <summary>
        /// Determining the size of the ship by the coordinates of the "bow's" and aft
        /// </summary>
        /// <param name="head">"Bow's" point</param>
        /// <param name="tail">Aft's point</param>
        /// <returns>Ship size</returns>
        private int GetShipSize(Point head, Point tail)
        {
            return (int)Math.Sqrt(Math.Pow(head.X - tail.X, expressionPower) + Math.Pow(head.Y - tail.Y, expressionPower));
        }

        /// <summary>
        /// Determines if the ship can be positioned at the specified coordinates
        /// </summary>
        /// <returns>
        /// "true" - the ship can be positioned at the specified coordinates
        /// "false" - the ship cannot be positioned at the specified coordinates
        /// </returns>
        private bool IsLocationValid(Point head, Point tail)
        {
            return IsPointInsideField(head)
                && IsPointInsideField(tail)
                && CheckDirection(head, tail)
                && IsNeighborhoodsAbsent(head, tail);
        }

        /// <summary>
        /// Defines a vertical or horizontal line
        /// </summary>
        /// <param name="head">"Bow's" point</param>
        /// <param name="tail">Aft's point</param>
        /// <returns>true - if the line is vertical / horizontal. false - if the line is at an angle.</returns>
        private bool CheckDirection(Point head, Point tail)
        {
            return (head.Equals(tail)) || IsHorizontalLine(head, tail) || IsVerticalLine(head, tail);
        }

        private static bool IsHorizontalLine(Point head, Point tail)
        {
            return head.Y == tail.Y;
        }

        private static bool IsVerticalLine(Point head, Point tail)
        {
            return head.X == tail.X;
        }

        /// <summary>
        /// Checking if at least one of the points is outside the map
        /// </summary>
        /// <param name="head">"Bow's" point</param>
        /// <param name="tail">Aft's point</param>
        /// <returns>true - all points are within the map, false - at least one point is outside the map</returns>
        private bool IsPointInsideField(Point point)
        {
            var battleFieldHalfSide = this.battleFieldSideLength / coefficient;
            return point.XAbsolute < battleFieldHalfSide && point.YAbsolute < battleFieldHalfSide;
        }

        private bool IsNeighborhoodsAbsent(Point head, Point tail)
        {
            var isNeighborFounded = false;
            var points = GetAllPointsForShip(head, tail);
            foreach (var point in points)
            {
                if (!CheckPointOnNeighbor(point))
                {
                    isNeighborFounded = true;
                    break;
                }
            }
            return !isNeighborFounded;
        }

        private bool CheckPointOnNeighbor(Point point)
        {
            if (GetCellByPoint(point).Ship != null)
            {
                return false;
            }
            return true;
        }

        private Cell GetCellByPoint(Point point)
        {
            return cells[point];
        }

        /// <summary>
        /// Creating points between head and tail
        /// </summary>
        /// <param name="head">"Bow's" point</param>
        /// <param name="tail">Aft's point</param>
        /// <returns>Collection of points between head and tail</returns>
        private List<Point> GetPointsBetween(Point head, Point tail)
        {
            var points = new List<Point>();

            if (IsHorizontalLine(head, tail))
            {
                if (head.X > tail.X) // head point to the right of the tail
                {
                    for (int x = tail.X; x <= head.X; x++)
                    {
                        points.Add(new Point(x, head.Y));
                    }
                }
                else // head point to the left of the tail
                {
                    for (int x = head.X; x <= tail.X; x++)
                    {
                        points.Add(new Point(x, head.Y));
                    }
                }
            }
            else if (IsVerticalLine(head, tail))
            {
                if (head.Y > tail.Y) // head point above of the tail
                {
                    for (int y = tail.Y; y <= head.Y; y++)
                    {
                        points.Add(new Point(head.X, y));
                    }
                }
                else //head point below of the tail
                {
                    for (int y = head.Y; y <= tail.Y; y++)
                    {
                        points.Add(new Point(head.X, y));
                    }
                }
            }
            else //ship-point
                return new List<Point>() { head };
            return points;
        }

        private List<Point> GetPointsAround(Point point)
        {
            return new List<Point>(4) {
                            new Point(point.X, point.Y + 1),
                            new Point(point.X, point.Y - 1),
                            new Point(point.X - 1, point.Y),
                            new Point(point.X + 1, point.Y)};
        }

        private List<Point> GetAllPointsForShip(Point head, Point tail)
        {
            var points = GetPointsBetween(head, tail);
            var additionalPoints = new List<Point>();
            foreach (var point in points)
            {
                additionalPoints.AddRange(GetPointsAround(point));
            }
            foreach (var point in additionalPoints)
            {
                if (IsPointInsideField(point))
                {
                    if (!points.Contains(point))
                    {
                        points.Add(point);
                    }
                }
            }
            return points;
        }

        /// <summary>
        /// Sorting of ships collection by distance to origin point
        /// </summary>
        public void SortShips()
        {
            var sortedShips = cells.Where(item => item.Value.Ship != null)
                                    .OrderBy(item => item.Value.DistanceToOrigin)
                                    .Select(item => item.Value.Ship)
                                    .Distinct();
            ships.Clear();
            ships.AddRange(sortedShips);
        }

        public override string ToString()
        {
            StringBuilder battleFieldState = new StringBuilder();
            foreach (var cell in cells.Where(item => item.Value.Ship != null))
            {
                battleFieldState.Append(cell.ToString());
            }
            return battleFieldState.ToString();
        }

        #endregion Methods
    }
}