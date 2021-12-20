using CustomIdentityAPI.Models;
using CustomIdentityAPI.Models.DTOs;
using CustomIdentityAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CustomIdentity2.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : Controller
    {
        private readonly UserManager<CustomIdentityUser> userManager;

        private readonly SignInManager<CustomIdentityUser> signInManager;
        private readonly TokenService tokenService;

        public AccountController(UserManager<CustomIdentityUser> userManager,
            SignInManager<CustomIdentityUser> signInManager,
            TokenService tokenService)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDataDto>> Login(UserLoginDto model)
        {
            if ((model.EmailProp is null || model.EmailProp == string.Empty)
                && (model.LoginProp is null || model.LoginProp == string.Empty))
            {
                ModelState.AddModelError("", "At least one property should be inputted (login or email).");
                return Unauthorized();
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
                    return Unauthorized();
                }

                var loginResult = await this.signInManager.CheckPasswordSignInAsync(user,
                                                                            model.Password,
                                                                            lockoutOnFailure: false);

                if (loginResult != null && loginResult.Succeeded)
                {
                    return GetUserDto(user);
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

        [HttpPost("register")]
        public async Task<ActionResult<UserDataDto>> Register(UserRegistrationDto model)
        {
            var user = new CustomIdentityUser { UserName = model.LoginProp, Email = model.EmailProp };
            var userEmail = await this.userManager.FindByEmailAsync(user.Email);
            var userLogin = await this.userManager.FindByNameAsync(user.UserName);

            if (userEmail != null)
            {
                return BadRequest($"\"{model.EmailProp}\" is already taken. Please check you email.");
            }
            else if (userLogin != null)
            {
                return BadRequest($"\"{model.LoginProp}\" is already taken. Please check you login.");
            }

            var createResult = await this.userManager.CreateAsync(user, model.Password);

            if (createResult.Succeeded)
            {
                //await this.signInManager.SignInAsync(user, false);
                return GetUserDto(user);
            }

            return BadRequest("Problem with user registration.");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await this.signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        private ActionResult<UserDataDto> GetUserDto(CustomIdentityUser user)
        {
            return new UserDataDto()
            {
                EmailProp = user.Email,
                LoginProp = user.UserName,
                Token = this.tokenService.CreateToken(user),
            };
        }
    }
}