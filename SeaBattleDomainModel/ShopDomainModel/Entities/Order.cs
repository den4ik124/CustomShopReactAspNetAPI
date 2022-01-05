using ShopDomainModel.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace ShopDomainModel.Entities
{
    public class Order : IOrder
    {
        public Order()
        {
            Products = new List<OrderProduct>();
            DateAndTimeOfCreation = DateTime.Now;
        }

        public Order(Guid id) : this()
        {
            Id = id;
        }

        public Guid Id { get; set; }

        public string OwnerId { get; set; }

        [Column(TypeName = "DateTime2")]
        public DateTime DateAndTimeOfCreation { get; private set; }

        public decimal TotalCost
        { get => this.Products.Sum(item => item.Product.Price * item.ProductCount); private set { } }

        public List<OrderProduct> Products { get; set; }

        public void AddProduct(IProduct product, int count)
        {
            if (count <= 0)
                return;
            NullCheck(product);
            this.Products.Add(new OrderProduct()
            {
                Product = (Product)product,
                ProductId = product.Id,
                ProductCount = count,
                Order = this,
                OrderId = this.Id
            });
        }

        public IProduct GetProduct(string title)
        {
            if (title.Length == 0)
                throw new Exception("Product title can not be empty");
            return Products.FirstOrDefault(prod => prod.Product.Title == title).Product;
        }

        public void RemoveProduct(IProduct product)
        {
            NullCheck(product);
            this.Products.Remove(Products.FirstOrDefault(prod => prod == product));
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

        private void NullCheck<T>(T item)
        {
            _ = item ?? throw new ArgumentNullException(nameof(item));
        }
    }
}