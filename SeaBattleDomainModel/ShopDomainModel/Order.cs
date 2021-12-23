using ShopDomainModel.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopDomainModel
{
    public class Order : IOrder
    {
        private class OrderItem
        {
            private readonly IProduct product;
            private readonly int count;

            public OrderItem(IProduct product, int count)
            {
                this.product = product ?? throw new ArgumentNullException();
                this.count = count;
            }

            public Guid ItemId { get => this.product.Id; }
            public IProduct Product => product;

            public int Count => count;

            public decimal TotalCost { get => this.product.Price * this.count; }
        }

        public Order()
        {
            Products = new List<OrderProduct>();
            DateAndTimeOfCreation = DateTime.Now;
        }

        public Order(Guid id)
        {
            Id = id;
            DateAndTimeOfCreation = DateTime.Now;
            Products = new List<OrderProduct>();
        }

        public Guid Id { get; set; }

        [Column(TypeName = "DateTime2")]
        public DateTime DateAndTimeOfCreation { get; private set; }

        public List<OrderProduct> Products { get; set; }

        //public void SetTestProducts()
        //{
        //    AddProduct(new Product(10.5M, "Mars"));
        //    AddProduct(new Product(9M, "Lion"));
        //    AddProduct(new Product(5M, "Kontik"));
        //    AddProduct(new Product(10M, "Super Kontik"));
        //    AddProduct(new Product(8M, "Waffles"));
        //    AddProduct(new Product(10.5M, "Twix"));
        //    AddProduct(new Product(19.5M, "Nuts"));
        //    AddProduct(new Product(12M, "Bounty"));
        //    AddProduct(new Product(15M, "Lion KingSize"));
        //}

        public void AddProduct(IProduct product)
        {
            NullCheck(product);
            this.Products.Add(new OrderProduct() { Product = (Product)product });
        }

        public IProduct GetProduct(string title)
        {
            return Products.FirstOrDefault(prod => prod.Product.Title == title).Product;
        }

        public void RemoveProduct(IProduct product)
        {
            NullCheck(product);
            this.Products.Remove(Products.FirstOrDefault(prod => prod == product));
        }

        private void NullCheck<T>(T item)
        {
            _ = item ?? throw new ArgumentNullException(nameof(item));
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var product in Products)
            {
                sb.Append(product.Product.ToString() + Environment.NewLine);
            }
            return sb.ToString();
        }
    }
}