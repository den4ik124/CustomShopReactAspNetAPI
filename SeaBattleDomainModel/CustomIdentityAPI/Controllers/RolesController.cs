using CustomIdentityAPI.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UserDomainModel;

namespace CustomIdentityAPI.Controllers
{
    [AllowAnonymous]
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
        [Authorize]
        [HttpGet()]
        public IEnumerable<CustomRoles> Roles()
        {
            return this.roleManager.Roles;
        }

        [HttpPost("AddRole")]
        public async Task<ActionResult<RoleDto>> AddRole(RoleDto role)
        {
            var newRole = new CustomRoles() { Name = role.RoleName };
            var result = await this.roleManager.CreateAsync(newRole);
            if (result.Succeeded)
            {
                return role;
            }
            var sbErrors = new StringBuilder();
            foreach (var error in result.Errors)
            {
                sbErrors.Append(error.Description + Environment.NewLine);
            }
            ModelState.AddModelError("", sbErrors.ToString());
            return ValidationProblem(ModelState);
        }

        [HttpDelete("delete/{id}")]
        public async Task<ActionResult> DeleteRole(Guid id)
        {
            var role = await this.roleManager.FindByIdAsync(id.ToString());
            if (role == null)
            {
                return ValidationProblem($"Role with {id} was not found");
            }
            await this.roleManager.DeleteAsync(role);
            return Ok("Role has been successfully removed");
        }

        [HttpPost("AddUser")]
        public async Task<ActionResult<CustomIdentityUser>> AddUserToRole(UserDataDto user)
        {
            var addedUser = await userManager.FindByNameAsync(user.LoginProp);
            var result = await userManager.AddToRoleAsync(addedUser, "Manager");
            //result = await userManager.AddToRoleAsync(addedUser, "Manager");

            return addedUser;
        }
    }
}