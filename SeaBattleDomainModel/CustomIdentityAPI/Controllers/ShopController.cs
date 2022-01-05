using Application.Products;
using CustomIdentityAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence;
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
        private readonly ShopDbContext context;
        #region Default implementation

        //private readonly ShopDbContext shopContext;

        //public ShopController(ShopDbContext shopContext)
        //{
        //    this.shopContext = shopContext;
        //}

        #endregion Default implementation

        public ShopController(ShopDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<IProduct>>> GetProducts()
        {
            return await this.Mediator.Send(new List.Query());
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<IProduct>> GetProduct(Guid id)
        {
            return await this.Mediator.Send(new ProductDetails.Query() {Id = id });
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(Product product)
        {
            return Ok(await Mediator.Send(new Create.Command { Product = product }));
        }

        [HttpPut("product_id{id}")]
        public async Task<ActionResult<Product>> UpdateProduct(Guid id, Product product)
        {
            product.Id = id;
            return Ok(await Mediator.Send(new Edit.Command() {Product = product }));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(Guid id)
        {
            return Ok(await Mediator.Send(new Delete.Command { Id = id }));
        }

        [HttpGet("testOrder")]
        public List<OrderItem> GetOrder()
        {
            var list = new List<OrderItem>()
            {
                new OrderItem()
                {
                    Product = new Product(11.11M, "Test order 1","Test description 1"),
                    ProductAmount = 1
                },
                new OrderItem()
                {
                    Product = new Product(22.22M, "Test order 2","Test description 2"),
                    ProductAmount = 2
                },
                new OrderItem()
                {
                    Product = new Product(33.33M, "Test order 3","Test description 3"),
                    ProductAmount = 3
                },
            };
            return list;
        }

        [HttpPost("createOrder")]
        public IActionResult GetOrderFromClient(IEnumerable<OrderItem> orderItems)
        {
            var order = new Order(Guid.NewGuid());
            foreach (var item in orderItems)
            {
                order.AddProduct(item.Product, item.ProductAmount);
                this.context.Attach(item.Product);
            }

            this.context.Orders.Add(order);
            this.context.SaveChanges();
            return Ok();
        }
    }
}