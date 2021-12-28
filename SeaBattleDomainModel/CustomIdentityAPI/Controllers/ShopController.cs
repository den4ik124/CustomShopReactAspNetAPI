using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence;
using ShopDomainModel.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace CustomIdentityAPI.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("[controller]")]
    public class ShopController : Controller
    {
        private readonly ShopDbContext shopContext;

        public ShopController(ShopDbContext shopContext)
        {
            this.shopContext = shopContext;
        }

        [HttpGet]
        public IEnumerable<IProduct> GetProducts()
        {
            return this.shopContext.Products.ToList();
        }
    }
}