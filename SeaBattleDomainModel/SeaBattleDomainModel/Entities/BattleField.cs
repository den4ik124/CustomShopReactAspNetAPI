using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleDomainModel.Entities
{
    public class BattleField
    {
        private readonly List<Ship> ships;

        private readonly int size;

        private Cell[] cells;

        public BattleField(int size)
        {
            this.size = size;
            this.ships = new List<Ship>();
            this.cells = FillCells(size);
        }

        public Ship this[Quadrant quadrant, int x, int y]
        {
            get
            {
                return GetShipByCoordinates(quadrant, x, y);
            }
        }

        private Cell[] FillCells(int size)
        {
            //TODO : реализовать метод заполнения ячеек
            //return new Cell[1];
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds a ship in the list of ships
        /// </summary>
        /// <returns>Ship from list</returns>
        private Ship GetShipByCoordinates(Quadrant quadrant, int x, int y)
        {
            return cells.First(cell => cell.Point.Quadrant == quadrant  //cell checking by quadrant
                            && cell.Point.XQuad == x                    //cell checking by quadrant X coordinate
                            && cell.Point.YQuad == y)                   //cell checking by quadrant Y coordinate
                            .Ship;                                      //Select ship from cell
        }

        public void AddShip(Ship ship, Point head, Point tail)
        {
            if (!IsLocationValid(head, tail))
            {
                return;
            }
            ship.Size = GetShipSize(head, tail);
            cells.Select(cell => cell.Point);
            //TODO: реализовать инициализацию и заполнение массива точек ссылками на корабль
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
            return CheckOutOfBoundaries(head, tail) && CheckOrientation(head, tail) && CheckCollision(head, tail) && CheckNeighborhoods(head, tail);
        }

        /// <summary>
        /// Defines a vertical or horizontal line
        /// </summary>
        /// <param name="head">"Bow's" point</param>
        /// <param name="tail">Aft's point</param>
        /// <returns>true - if the line is vertical / horizontal. false - if the line is at an angle.</returns>
        private bool CheckOrientation(Point head, Point tail)
        {
            return IsHorizontalLine(head, tail) || IsVerticalLine(head, tail);
        }

        private static bool IsHorizontalLine(Point head, Point tail)
        {
            return head.X != tail.X && head.Y == tail.Y;
        }

        private static bool IsVerticalLine(Point head, Point tail)
        {
            return head.Y != tail.Y && head.X == tail.X;
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
                if (Array.Find(cells, (cell) => cell.Point.Equals(point)).Ship != null)
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
            return head.X <= size / 2 && head.X >= -size / 2
                && head.Y <= size / 2 && head.Y >= -size / 2
                && tail.X <= size / 2 && tail.X >= -size / 2
                && tail.Y <= size / 2 && tail.Y >= -size / 2;
        }

        private bool CheckNeighborhoods(Point head, Point tail)
        {
            bool neighborFlag = false;
            var points = GetPointsBetween(head, tail);
            foreach (var point in points)
            {
                if (CheckPointOnNeighbor(point) == true)
                {
                    neighborFlag = false;
                    break;
                }
            }
            return neighborFlag;
        }

        private bool CheckPointOnNeighbor(Point point)
        {
            bool isNeighborAbsent = true; //переменная, показывающая найден ли сосед вокруг указанной точки
            Cell cellCurrent = GetCellByPoint(point);
            Cell[] cellsForCheck = new Cell[4] {
                Array.Find(cells, (cell) => cell.Point.Y == point.Y + 1), //ячейка, содержащая точку выше point
                Array.Find(cells, (cell) => cell.Point.Y == point.Y - 1), //ячейка, содержащая точку ниже point
                Array.Find(cells, (cell) => cell.Point.X == point.X - 1), //ячейка, содержащая точку левее point
                Array.Find(cells, (cell) => cell.Point.X == point.X + 1) //ячейка, содержащая точку правее point
            };

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
            return Array.Find(cells, (cell) => cell.Point.Equals(point)); //ячейка, содержащая точку point
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
                if (head.X > tail.X) //если голова "правее" хвоста в системе координат
                {
                    for (int x = tail.X; x != head.X; x++)
                    {
                        points.Add(new Point(x, head.Y));
                    }
                }
                else //если голова "левее" хвоста в системе координат
                {
                    for (int x = head.X; x != tail.X; x++)
                    {
                        points.Add(new Point(x, head.Y));
                    }
                }
            }
            else if (IsVerticalLine(head, tail))
            {
                if (head.Y > tail.Y) //если голова "выше" хвоста в системе координат
                {
                    for (int y = tail.Y; y != head.Y; y++)
                    {
                        points.Add(new Point(head.X, y));
                    }
                }
                else //если голова "ниже" хвоста в системе координат
                {
                    for (int y = head.Y; y != tail.Y; y++)
                    {
                        points.Add(new Point(head.X, y));
                    }
                }
            }
            else //ship-point
                return new List<Point>() { head };
            return points;
        }

        public override string ToString()
        {
            //TODO: реализовать переопределение
            StringBuilder battleFieldState = new StringBuilder();
            foreach (var cell in cells)
            {
                battleFieldState.Append(cell.ToString());
            }
            return battleFieldState.ToString();
        }
    }
}