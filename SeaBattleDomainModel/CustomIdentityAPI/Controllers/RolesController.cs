using CustomIdentityAPI.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserDomainModel;

namespace CustomIdentityAPI.Controllers
{

    public class RolesController : BaseIdentityController
    {
        public RolesController(UserManager<CustomIdentityUser> userManager,
            RoleManager<CustomRoles> roleManager) : base(userManager, roleManager)
        {
        }

        [HttpGet()]
        public IEnumerable<CustomRoles> Roles()
        {
            return RoleManager.Roles;
        }

        [HttpPost("AddRole")]
        public async Task<ActionResult<RoleDto>> AddRole(RoleDto role)
        {
            var newRole = new CustomRoles(role.RoleName);
            var result = await RoleManager.CreateAsync(newRole);
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
            var role = await RoleManager.FindByIdAsync(id.ToString());
            if (role == null)
            {
                return ValidationProblem($"Role with {id} was not found");
            }
            await RoleManager.DeleteAsync(role);
            return Ok("Role has been successfully removed");
        }


        [HttpPut("addRoles_{userName}")]
        public async Task<ActionResult<CustomIdentityUser>> AddUserToRole(string userName, IEnumerable<string> roles)
        {
            var addedUser = await UserManager.FindByNameAsync(userName);
            foreach (var role in roles)
            {
                await UserManager.AddToRoleAsync(addedUser, role);
            }
            return addedUser;
        }

        [HttpPut("removeRole_{userName}")]
        public async Task<ActionResult<CustomIdentityUser>> RemoveRoleFromUser(string userName, IEnumerable<string> role)
        {
            var addedUser = await UserManager.FindByNameAsync(userName);

            await UserManager.RemoveFromRoleAsync(addedUser, role.First());

            return addedUser;
        }
    }
}