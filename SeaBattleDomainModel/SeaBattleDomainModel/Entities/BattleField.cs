using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleDomainModel.Entities
{
    public class BattleField
    {
        #region Fields

        //private readonly List<Ship> ships;
        private readonly Dictionary<Cell, Ship> ships;

        private readonly int battleFieldSideLength;

        private Dictionary<Point, Cell> cells;

        #endregion Fields

        #region Constructors

        public BattleField(int fieldSideLength)
        {
            this.battleFieldSideLength = fieldSideLength + 1;
            //this.ships = new List<Ship>();
            this.ships = new Dictionary<Cell, Ship>();

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
            int battleFieldHalfSide = battleFieldSideLength / 2;
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
                this.ships.Add(cell, ship);
            }

            //this.ships.Add(ship);
        }

        /// <summary>
        /// Determining the size of the ship by the coordinates of the "bow's" and aft
        /// </summary>
        /// <param name="head">"Bow's" point</param>
        /// <param name="tail">Aft's point</param>
        /// <returns>Ship size</returns>
        private int GetShipSize(Point head, Point tail)
        {
            return (int)Math.Sqrt(Math.Pow(head.X - tail.X, 2) + Math.Pow(head.Y - tail.Y, 2));
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
            return CheckOutOfBoundaries(head, tail) && CheckDirection(head, tail) /*&& CheckCollision(head, tail)*/ && CheckNeighborhoods(head, tail);
        }

        /// <summary>
        /// Defines a vertical or horizontal line
        /// </summary>
        /// <param name="head">"Bow's" point</param>
        /// <param name="tail">Aft's point</param>
        /// <returns>true - if the line is vertical / horizontal. false - if the line is at an angle.</returns>
        private bool CheckDirection(Point head, Point tail)
        {
            //return (head.Equals(tail)) || IsHorizontalLine(head, tail) || IsVerticalLine(head, tail);
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
        /// The method checks if there are any other ships (other points) between the specified points
        /// </summary>
        /// <param name="head">"Bow's" point</param>
        /// <param name="tail">Aft's point</param>
        /// <returns>true - there is no collision, false - there is an collision</returns>
        private bool CheckCollision(Point head, Point tail)
        {
            bool isNoCollision = true;
            var points = GetPointsBetween(head, tail);
            foreach (var point in points)
            {
                if (cells[point].Ship != null) //TODO : проверить что не так с != null
                {
                    isNoCollision = false;
                    break;
                }
            }
            return isNoCollision;
        }

        /// <summary>
        /// Checking if at least one of the points is outside the map
        /// </summary>
        /// <param name="head">"Bow's" point</param>
        /// <param name="tail">Aft's point</param>
        /// <returns>true - all points are within the map, false - at least one point is outside the map</returns>
        private bool CheckOutOfBoundaries(Point head, Point tail)
        {
            int bfHalfLength = this.battleFieldSideLength / 2;
            return head.XAbsolute < bfHalfLength || head.YAbsolute < bfHalfLength
                && tail.XAbsolute < bfHalfLength || tail.YAbsolute < bfHalfLength;
        }

        private bool CheckNeighborhoods(Point head, Point tail)
        {
            var isNeighborFounded = false;
            var points = GetPointsBetween(head, tail);
            var additionalPoints = new List<Point>();
            foreach (var point in points)
            {
                additionalPoints.Add(new Point(point.X, point.Y + 1));
                additionalPoints.Add(new Point(point.X, point.Y - 1));
                additionalPoints.Add(new Point(point.X - 1, point.Y));
                additionalPoints.Add(new Point(point.X + 1, point.Y));
            }
            foreach (var point in additionalPoints)
            {
                if (!points.Contains(point))
                {
                    points.Add(point);
                }
            }

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
            bool isNeighborAbsent = true;
            //TODO : проверить, если корабль стоит впритык к границе
            Cell[] cellsForCheck = new Cell[4] {
                cells[new Point(point.X,point.Y + 1)],  //cell above specified point
                cells[new Point(point.X,point.Y - 1)],  //cell below specified point
                cells[new Point(point.X-1,point.Y)],    //cell to the left of the specified point
                cells[new Point(point.X+1,point.Y)] };  //cell to the right of the specified point

            foreach (var cell in cellsForCheck)
            {
                if (cell == null)
                {
                    continue;
                }
                if (cell.Ship != null)
                {
                    isNeighborAbsent = false;
                    break;
                }
            }
            return isNeighborAbsent;
        }

        private Cell GetCellByPoint(Point point)
        {
            return cells[point];
            //return Array.Find(cells, (cell) => cell.Point.Equals(point)); //Cell that include point inside
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

        /// <summary>
        /// Sorting of ships collection by distance to origin point
        /// </summary>
        public void SortShips()
        {
            //TODO : Implement ship collection sorting

            List<Ship> sortedShips = new List<Ship>(ships.Count);
            var groupOfCellsWithShip = cells.Where(cell => cell.Value.Ship != null)
                                    .OrderBy(item => item.Value.DistanceToOrigin)
                                    .GroupBy((point) => point.Value.Ship);

            foreach (var group in groupOfCellsWithShip)
            {
                sortedShips.Add(ships[group.First().Value]);
            }
        }

        public override string ToString()
        {
            StringBuilder battleFieldState = new StringBuilder();
            foreach (var cell in cells)
            {
                battleFieldState.Append(cell.ToString());
            }
            return battleFieldState.ToString();
        }

        #endregion Methods
    }
}