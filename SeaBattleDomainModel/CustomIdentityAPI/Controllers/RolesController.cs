using CustomIdentityAPI.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserDomainModel;

namespace CustomIdentityAPI.Controllers
{
    //[AllowAnonymous]
    //[Authorize(Roles = "Admin")]
    [ApiController]
    [Route("[controller]")]
    public class RolesController : Controller
    {
        private readonly UserManager<CustomIdentityUser> userManager;
        private readonly RoleManager<CustomRoles> roleManager;

        public RolesController(UserManager<CustomIdentityUser> userManager,
            RoleManager<CustomRoles> roleManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        //[Authorize(Roles = "Admin")]
        [Authorize(Roles = "Admin")]
        [HttpGet()]
        public IEnumerable<CustomRoles> Roles()
        {
            var request = base.HttpContext.Request;
            var response = base.HttpContext.Response;

            return this.roleManager.Roles;
        }

        [HttpPost]
        public async Task<RoleDto> AddRole(RoleDto role)
        {
            var newRole = new CustomRoles() { Name = role.RoleName };
            var result = await this.roleManager.CreateAsync(newRole);

            return role;
        }

        [HttpPost("AddUser")]
        public async Task<CustomIdentityUser> AddUserToRole(UserDataDto user)
        {
            var addedUser = await userManager.FindByNameAsync(user.LoginProp);
            var result = await userManager.AddToRoleAsync(addedUser, "test role");

            return addedUser;
        }
    }
}