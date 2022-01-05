using ShopDomainModel.Interfaces;
using System;
using System.Collections.Generic;

namespace ShopDomainModel.Entities
{
    public class Product : IProduct
    {
        public Product()
        {
        }

        public Product(decimal price, string title, string description = "")
        {
            Id = Guid.NewGuid();
            Price = price;
            Title = title;
            Description = description;
        }

        public Guid Id { get; set; }
        public decimal Price { get; set; }

        public string Title { get; set; }

        public List<OrderProduct> Orders { get; set; }

        public string Description { get; set; }

        public override string ToString()
        {
            return $"{this.Title} : {this.Price}";
        }
    }
}