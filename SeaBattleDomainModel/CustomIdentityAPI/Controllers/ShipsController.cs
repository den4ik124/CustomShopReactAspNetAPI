using Microsoft.AspNetCore.Mvc;

namespace CustomIdentityAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ShipsController : ControllerBase
    {
        public ShipsController()
        {
        }

        //[HttpGet]
        //public IEnumerable<Ship> Get()
        //{
        //    return this.shipData.GetShips();
        //}
    }
}