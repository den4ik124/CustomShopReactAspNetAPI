using System;

namespace SeaBattleDomainModel.Entities
{
    [Flags]
    public enum Quadrant
    {
        First = 1,
        Second = 2,
        Third = 4,
        Fourth = 8
    }
}