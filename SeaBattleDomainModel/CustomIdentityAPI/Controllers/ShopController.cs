using Application.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopDomainModel.Entities;
using ShopDomainModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CustomIdentityAPI.Controllers
{
    [AllowAnonymous]
    public class ShopController : BaseApiController
    {
        #region Default implementation

        //private readonly ShopDbContext shopContext;

        //public ShopController(ShopDbContext shopContext)
        //{
        //    this.shopContext = shopContext;
        //}

        #endregion Default implementation

        [HttpGet]
        public async Task<ActionResult<IEnumerable<IProduct>>> GetProducts()
        {
            return await this.Mediator.Send(new List.Query());
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(Product product)
        {
            return Ok(await Mediator.Send(new Create.Command { Product = product }));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(Guid id)
        {
            return Ok(await Mediator.Send(new Delete.Command { Id = id }));
        }
    }
}