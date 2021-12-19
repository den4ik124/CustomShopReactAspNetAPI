using CustomIdentityAPI.Models;
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

        public AccountController(UserManager<CustomIdentityUser> userManager, SignInManager<CustomIdentityUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserLoginDto>> Login(UserLoginDto model)
        {
            if (model.EmailProp is null && model.LoginProp is null)
            {
                ModelState.AddModelError("", "At least one property should be inputted (login or email).");
                return Unauthorized();
            }
            if (ModelState.IsValid)
            {
                Microsoft.AspNetCore.Identity.SignInResult loginResult = null;
                if (model.LoginProp != null)
                {
                    loginResult = await this.signInManager.PasswordSignInAsync(model.LoginProp,
                                                                                    model.Password,
                                                                                    false,
                                                                                    lockoutOnFailure: false);
                }
                else if (model.EmailProp != null)
                {
                    var userByEmail = await this.userManager.FindByEmailAsync(model.EmailProp);
                    loginResult = await this.signInManager.PasswordSignInAsync(userByEmail.UserName,
                                                                                    model.Password,
                                                                                    false,
                                                                                    lockoutOnFailure: false);
                }

                if (loginResult != null && loginResult.Succeeded)
                {
                    if (Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return new UserLoginDto()
                        {
                            EmailProp = model.EmailProp,
                            LoginProp = model.LoginProp,
                        };
                    }
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
        public async Task<ActionResult<UserRegistrationDto>> Register(UserRegistrationDto model)
        {
                var user = new CustomIdentityUser { UserName = model.LoginProp, Email = model.EmailProp };
                var userEmail = await this.userManager.FindByEmailAsync(user.Email);

                if (userEmail != null)
                {
                    return BadRequest($"\"{model.EmailProp}\" is already taken. Please check you email.");
                }

                var createResult = await this.userManager.CreateAsync(user, model.Password);

                if (createResult.Succeeded)
                {
                    await this.signInManager.SignInAsync(user, false);

                    return new UserRegistrationDto()
                    {
                        EmailProp = model.EmailProp,
                        LoginProp = model.LoginProp
                    };
                }

            return BadRequest("Problem with user registration.");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await this.signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}