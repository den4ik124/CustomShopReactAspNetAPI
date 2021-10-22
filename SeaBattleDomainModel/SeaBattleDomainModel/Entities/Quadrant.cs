using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleDomainModel.Entities
{
    [Flags]
    public enum Quadrant
    {
        I = 1,
        II = 2,
        III = 4,
        IV = 8
    }
}