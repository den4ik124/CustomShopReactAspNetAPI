using CustomIdentity.Data;
using CustomIdentity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CustomIdentity.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly IShipData shipData;

        public HomeController(ILogger<HomeController> logger, IShipData shipData)
        {
            this.logger = logger;
            this.shipData = shipData;
        }

        [Authorize]
        public IActionResult Index()
        {
            var ships = this.shipData.GetShips();
            return View(ships);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}