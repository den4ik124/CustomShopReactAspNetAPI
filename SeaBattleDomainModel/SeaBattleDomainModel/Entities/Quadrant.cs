﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleDomainModel.Entities
{
    [Flags]
    public enum Quadrant
    {
        //TODO (DONE): Let's rename, i.e. First, Second ...

        First = 1,
        Second = 2,
        Third = 4,
        Fourth = 8
    }
}