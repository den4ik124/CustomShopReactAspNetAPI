using CustomIdentityAPI.Models;
using CustomIdentityAPI.Models.DTOs;
using CustomIdentityAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UserDomainModel;

namespace CustomIdentity2.Controllers
{
    
    [ApiController]
    [Route("[controller]")]
    public class AccountController : Controller
    {
        private readonly UserManager<CustomIdentityUser> userManager;

        private readonly RoleManager<CustomRoles> rolemanager;
        private readonly SignInManager<CustomIdentityUser> signInManager;

        private readonly TokenService tokenService;

        public AccountController(UserManager<CustomIdentityUser> userManager,
            RoleManager<CustomRoles> rolemanager,
            SignInManager<CustomIdentityUser> signInManager,
            TokenService tokenService)
        {
            this.userManager = userManager;
            this.rolemanager = rolemanager;
            this.signInManager = signInManager;
            this.tokenService = tokenService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<UserDataDto>> Login(UserLoginDto model)
        {
            if ((model.EmailProp is null || model.EmailProp == string.Empty)
                && (model.LoginProp is null || model.LoginProp == string.Empty))
            {
                ModelState.AddModelError("", "At least one property should be inputted (login or email).");
                return Unauthorized();
            }

            if (model.EmailProp != null && !IsEmailValid(model.EmailProp))
            {
                return ValidationProblem("Email has incorrect format!");
            }

            if (ModelState.IsValid)
            {
                CustomIdentityUser user = null;
                if (model.LoginProp != null && model.LoginProp != string.Empty)
                {
                    user = await this.userManager.FindByNameAsync(model.LoginProp);
                }
                else if (model.EmailProp != null && model.EmailProp != string.Empty)
                {
                    user = await this.userManager.FindByEmailAsync(model.EmailProp);
                }

                if (user == null)
                {
                    return ValidationProblem("User has not been founded");
                    //return Unauthorized();
                }

                var loginResult = await this.signInManager.CheckPasswordSignInAsync(user,
                                                                            model.Password,
                                                                            lockoutOnFailure: false);

                if (loginResult != null && loginResult.Succeeded)
                {
                    return await GetUserDto(user);
                }
                else
                {
                    ModelState.AddModelError("", "Login or password is invalid!");
                    return Unauthorized();
                }
            }

            ModelState.AddModelError("", "Пользователь не найден");
            return Unauthorized();
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<UserDataDto>> Register(UserRegistrationDto model)
        {
            if (!IsEmailValid(model.EmailProp))
            {
                return ValidationProblem("Email has incorrect format!");
            }

            var user = new CustomIdentityUser { UserName = model.LoginProp, Email = model.EmailProp };
            var userEmail = await this.userManager.FindByEmailAsync(user.Email);

            if (userEmail != null)
            {
                return BadRequest($"\"{model.EmailProp}\" is already taken. Please check your email.");
            }

            var userLogin = await this.userManager.FindByNameAsync(user.UserName);
            if (userLogin != null)
            {
                return BadRequest($"\"{model.LoginProp}\" is already taken. Please check your login.");
            }

            var createResult = await this.userManager.CreateAsync(user, model.Password);

            if (createResult.Succeeded)
            {
                return await GetUserDto(user);
            }

            return BadRequest("Problem with user registration.");
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<UserDataDto>> GetCurrentUser()
        {
            var user = await this.userManager.FindByEmailAsync(User.FindFirstValue(ClaimTypes.Email));
            return await GetUserDto(user);
        }

        //[AllowAnonymous]
        [Authorize(Roles = "Admin")]
        [HttpGet("users")]
        public async Task<IEnumerable<UserDataDto>> GetUsers()
        {
            var result = new List<UserDataDto>();
            var users = this.userManager.Users.ToList();
            foreach (var user in users)
            {
                result.Add(await GetUserDto(user));
            }
            return result;
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await this.signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        private bool IsEmailValid(string email)
        {
            string pattern = @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
            return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
        }

        private async Task<UserDataDto> GetUserDto(CustomIdentityUser user)
        {
            //IList<string> userRoles = await GetUserRoles(user);

            return new UserDataDto()
            {
                EmailProp = user.Email,
                LoginProp = user.UserName,
                Token = this.tokenService.CreateToken(user),
                Roles = await this.userManager.GetRolesAsync(user),
            };
        }

        //private async Task<IList<string>> GetUserRoles(CustomIdentityUser user)
        //{
        //    var roles = this.rolemanager.Roles.ToList();
        //    var userRoles = await this.userManager.GetRolesAsync(user); //new List<string>();
        //foreach (var role in roles)
        //{
        //    if (await this.userManager.IsInRoleAsync(user, role.Name))
        //    {
        //        userRoles.Add(role.Name);
        //    }
        //}
        //    return userRoles;
        //}
    }
}