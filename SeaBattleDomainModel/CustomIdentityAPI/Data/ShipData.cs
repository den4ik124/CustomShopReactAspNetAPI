using OrmRepositoryUnitOfWork.Interfaces;
using SeaBattleDomainModel.Entities;
using System.Collections.Generic;

namespace CustomIdentity.Data
{
    public class ShipData : IShipData
    {
        private IUnitOfWork unitOfWork;

        public ShipData(IUnitOfWork uow)
        {
            this.unitOfWork = uow;
        }

        public IEnumerable<Ship> GetShips()
        {
            return this.unitOfWork.ReadItems<Ship>();
        }
    }
}