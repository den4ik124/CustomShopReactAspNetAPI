using CustomIdentity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CustomIdentity2.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<CustomIdentityUser> userManager;

        private readonly SignInManager<CustomIdentityUser> signInManager;

        public AccountController(UserManager<CustomIdentityUser> userManager, SignInManager<CustomIdentityUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            return View(new UserLoginModel()
            {
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        public async Task<IActionResult> Login(UserLoginModel model)
        {
            if (model.EmailProp is null && model.LoginProp is null)
            {
                ModelState.AddModelError("", "At least one property should be inputted (login or email).");
                return View(model);
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
                        return Redirect(model.ReturnUrl);
                    }

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Login or password is invalid!");
                    return View(model);
                }
            }

            ModelState.AddModelError("", "Пользователь не найден");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(UserRegistrationModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new CustomIdentityUser { UserName = model.LoginProp, Email = model.EmailProp };
                var userEmail = await this.userManager.FindByEmailAsync(user.Email);

                if (userEmail != null)
                {
                    ModelState.AddModelError("", $"\"{model.EmailProp}\" is already taken. Pleash check you email.");
                    return View(model);
                }

                var createResult = await this.userManager.CreateAsync(user, model.Password);

                if (createResult.Succeeded)
                {
                    await this.signInManager.SignInAsync(user, false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var identityError in createResult.Errors)
                    {
                        ModelState.AddModelError("", identityError.Description);
                    }
                }
            }

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await this.signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}