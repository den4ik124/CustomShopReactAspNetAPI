using ShopDomainModel.Entities;
using System;
using System.Collections.Generic;

namespace ShopDomainModel.Interfaces
{
    public interface IProduct
    {
        public Guid Id { get; }
        decimal Price { get; set; }
        string Title { get; set; }

        public List<OrderProduct> Orders { get; set; }
    }
}