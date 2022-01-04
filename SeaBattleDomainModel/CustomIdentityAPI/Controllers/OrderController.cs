using Application.Products;
using CustomIdentityAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CustomIdentityAPI.Controllers
{
    public class OrderController : BaseApiController
    {



        [HttpPost("cart")]
        public async Task<IActionResult> CreateNewOrder(List<OrderItem> orderItems)
        {

            return Ok(await Mediator.Send(new Create.Command { }));
        }
    }
}
