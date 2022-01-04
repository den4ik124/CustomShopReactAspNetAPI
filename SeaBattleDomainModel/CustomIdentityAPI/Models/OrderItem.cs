using ShopDomainModel.Entities;

namespace CustomIdentityAPI.Models
{
    public class OrderItem
    {
        public Product Product { get; set; }
        public int ProductAmount { get; set; }
    }
}
