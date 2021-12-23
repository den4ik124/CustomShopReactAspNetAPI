using ShopDomainModel.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopDomainModel
{
    public class Product : IProduct
    {
        public Product()
        {
        }

        public Product(decimal price, string title)
        {
            Id = Guid.NewGuid();
            Price = price;
            Title = title;
        }

        public Guid Id { get; set; }
        public decimal Price { get; set; }

        public string Title { get; set; }

        public List<OrderProduct> Orders { get; set; }

        public override string ToString()
        {
            return $"{this.Title} : {this.Price}";
        }
    }
}