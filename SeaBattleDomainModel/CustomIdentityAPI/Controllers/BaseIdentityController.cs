using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserDomainModel;

namespace CustomIdentityAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BaseIdentityController : Controller
    {
        private readonly UserManager<CustomIdentityUser> userManager;
        private readonly RoleManager<CustomRoles> roleManager;

        public BaseIdentityController(UserManager<CustomIdentityUser> userManager,
    RoleManager<CustomRoles> roleManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        public UserManager<CustomIdentityUser> UserManager => userManager;

        public RoleManager<CustomRoles> RoleManager => roleManager;
    }
}
