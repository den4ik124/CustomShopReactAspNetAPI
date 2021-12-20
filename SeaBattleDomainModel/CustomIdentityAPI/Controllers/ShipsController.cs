using CustomIdentity.Data;
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
        private IShipData shipData;

        public ShipsController(IShipData shipData)
        {
            this.shipData = shipData;
        }

        [HttpGet]
        [Authorize]
        public IEnumerable<Ship> Get()
        {
            return this.shipData.GetShips();
        }
    }
}