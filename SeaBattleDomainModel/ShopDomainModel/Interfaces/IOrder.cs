using ShopDomainModel.Entities;
using System.Collections.Generic;

namespace ShopDomainModel.Interfaces
{
    public interface IOrder
    {
        public List<OrderProduct> Products { get; set; }

        void AddProduct(IProduct product, int count);

        void RemoveProduct(IProduct product);

        IProduct GetProduct(string title);
    }
}