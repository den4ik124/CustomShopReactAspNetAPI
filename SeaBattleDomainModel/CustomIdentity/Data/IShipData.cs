using SeaBattleDomainModel.Entities;
using System.Collections.Generic;

namespace CustomIdentity.Data
{
    public interface IShipData
    {
        IEnumerable<Ship> GetShips();
    }
}