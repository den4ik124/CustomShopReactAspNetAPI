using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SeaBattleDomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

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