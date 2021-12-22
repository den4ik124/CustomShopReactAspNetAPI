using ShopDomainModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopDomainModel
{
    public class Order : IOrder
    {
        public Order()
        {
            Products = new Dictionary<string, IProduct>();
            ProductsList = new List<Product>();

        }

        public Order(Guid id)
        {
            Id = id;
            Products = new Dictionary<string,IProduct>();
            ProductsList = new List<Product>();
            DateAndTimeOfCreation = DateTime.Now;
        }

        public Guid Id { get; set; }

        public DateTime DateAndTimeOfCreation { get; }

        public List<Product> ProductsList { get; set; }

        public Dictionary<string, IProduct> Products { get;}

        public void SetTestProducts()
        {
            AddProduct(new Product(10.5M, "Mars"));
            AddProduct(new Product(9M, "Lion"));
            AddProduct(new Product(5M, "Kontik"));
            AddProduct(new Product(10M, "Super Kontik"));
            AddProduct(new Product(8M, "Waffles"));
            AddProduct(new Product(10.5M, "Twix"));
            AddProduct(new Product(19.5M, "Nuts"));
            AddProduct(new Product(12M, "Bounty"));
            AddProduct(new Product(15M, "Lion KingSize"));
        }

        public void AddProduct(IProduct product)
        {
            NullCheck(product);
            this.Products.Add(product.Title, product);
            this.ProductsList.Add((Product)product);
        }

        public IProduct GetProduct(string title)
        {
            return Products[title];
        }

        public void RemoveProduct(IProduct product)
        {
            NullCheck(product);
            this.Products.Remove(product.Title);
            this.ProductsList.Remove((Product)product);
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
                sb.Append(product.Value.ToString() + Environment.NewLine);
            }
            return sb.ToString();
        }
    }
}
