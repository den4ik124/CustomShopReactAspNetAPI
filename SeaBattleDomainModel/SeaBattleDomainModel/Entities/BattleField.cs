using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleDomainModel.Entities
{
    public class BattleField
    {
        private List<Ship> ships;

        private int size;

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
            return new Cell[1];
        }

        /// <summary>
        /// Находит корабль в списке кораблей
        /// </summary>
        /// <returns>Корабль по переданным параметрам</returns>
        private Ship GetShipByCoordinates(Quadrant quadrant, int x, int y)
        {
            return cells.First(cell => cell.Point.Quadrant == quadrant  //проверка ячейки по квадранту
                            && cell.Point.XQuad == x                    //проверка ячейки по координате Х квадранта
                            && cell.Point.YQuad == y)                   //проверка ячейки по координате Y квадранта
                            .Ship;                                      //возврат корабля из ячейки
        }

        public void AddShip(Ship ship, Point head, Point tail)
        {
            if (IsLocationValid(head, tail) == false) //TODO: уточнить: оставить явную проверку на false (мне так больше нравится, т.к. читабельнее) или просто оставить метод без ==false?
            {
                return;
            }
            ship.Size = GetShipSize(head, tail);
            cells.Select(cell => cell.Point);
            //TODO: реализовать инициализацию и заполнение массива точек ссылками на корабль
            this.ships.Add(ship);
        }

        /// <summary>
        /// Определение размера корабля по координатам "носа" и кормы
        /// </summary>
        /// <param name="head">Точка "носа" корабля</param>
        /// <param name="tail">Точка кормы корабля</param>
        /// <returns>Размер корабля</returns>
        private int GetShipSize(Point head, Point tail)
        {
            return (int)Math.Sqrt(Math.Pow(head.X - tail.X, 2) + Math.Pow(head.Y - tail.Y, 2));
        }

        /// <summary>
        /// Определяет можно ли расположить корабль по указанным координатам
        /// </summary>
        /// <returns>
        /// true - корабль можно расположить по указанным координатам
        /// false - корабль нельзя расположить по указанным координатам
        /// </returns>
        private bool IsLocationValid(Point head, Point tail)
        {
            return CheckOutOfBoundaries(head, tail) && CheckOrientation(head, tail) && CheckCollision(head, tail) && CheckNeighborhoods(head, tail);
        }

        /// <summary>
        /// Определяет вертикальный или горизонтальный отрезок
        /// </summary>
        /// <param name="head">Точка "носа" корабля</param>
        /// <param name="tail">Точка кормы корабля</param>
        /// <returns>true - если линия вертикальная/горизонтальная. false - если линия под углом.</returns>
        private bool CheckOrientation(Point head, Point tail)
        {
            return head.X != tail.X && head.Y == tail.Y        //горизонтальная линия
                || head.Y != tail.Y && head.X == tail.X;       //вертикальная линия
        }

        /// <summary>
        /// Метод проверяет нет ли между указанными точками других кораблей (других точек)
        /// </summary>
        /// <param name="head">Координаты "носа" корабля</param>
        /// <param name="tail">Координаты кормы корабля</param>
        /// <returns>true - если пересечения нет, false - если пересечение есть</returns>
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
        /// Проверка не находится ли хотя бы одна из точек за пределами карты
        /// </summary>
        /// <param name="head">Координаты "носа" корабля</param>
        /// <param name="tail">Координаты кормы корабля</param>
        /// <returns>true - если все точки в пределах карты, false - хотя бы одна точка за пределами карты</returns>
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
        /// Создание точек между носом и кормой
        /// </summary>
        /// <param name="head">Точка носа</param>
        /// <param name="tail">Точка кормы</param>
        /// <returns>Коллекцию точек между носом и кормой</returns>
        private List<Point> GetPointsBetween(Point head, Point tail)
        {
            var points = new List<Point>();

            if (head.X != tail.X && head.Y == tail.Y) //горизонтальная линия
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
            else if (head.Y != tail.Y && head.X == tail.X)//вертикальная линия
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
            else //точка
                return new List<Point>() { head };
            return points;
        }

        public override string ToString()
        {
            //TODO: реализовать переопределение
            return base.ToString();
        }
    }
}