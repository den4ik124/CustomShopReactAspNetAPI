using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleDomainModel.Entities
{
    public class Cell
    {
        public Cell(Ship ship, Point point)
        {
            Ship = ship;
            Point = point;
        }

        public Ship Ship { get; set; }

        public Point Point { get; set; }
    }
}