using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopDomainModel.Interfaces
{
    public interface IOrder
    {
        public List<Product> ProductsList { get; set; }
        void AddProduct(IProduct product);
        void RemoveProduct(IProduct product);

        IProduct GetProduct(string title);
    }


}
