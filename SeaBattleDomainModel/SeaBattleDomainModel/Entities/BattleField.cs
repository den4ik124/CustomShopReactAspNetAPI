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
            if (head.X != tail.X && head.Y == tail.Y //горизонтальная линия
                || head.Y != tail.Y && head.X == tail.Y)
                return true;
            return false;
        }

        /// <summary>
        /// Метод проверяет нет ли между указанными точками других кораблей (других точек)
        /// </summary>
        /// <param name="head">Координаты "носа" корабля</param>
        /// <param name="tail">Координаты кормы корабля</param>
        /// <returns>true - если пересечения нет, false - если пересечение есть</returns>
        private bool CheckCollision(Point head, Point tail)
        {
            //TODO: реализовать проверку пересечения
            return true;
        }

        /// <summary>
        /// Проверка не находится ли хотя бы одна из точек за пределами карты
        /// </summary>
        /// <param name="head">Координаты "носа" корабля</param>
        /// <param name="tail">Координаты кормы корабля</param>
        /// <returns>true - если все точки в пределах карты, false - хотя бы одна точка за пределами карты</returns>
        private bool CheckOutOfBoundaries(Point head, Point tail)
        {
            //TODO: реализовать проверку выхода за карту
            return true;
        }

        private bool CheckNeighborhoods(Point head, Point tail)
        {
            //TODO: реализовать проверку на расположение кораблей с интервалом
            return true;
        }

        public override string ToString()
        {
            //TODO: реализовать переопределение
            return base.ToString();
        }
    }
}