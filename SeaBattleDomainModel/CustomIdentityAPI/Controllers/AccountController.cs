﻿using CustomIdentityAPI.Models;
using CustomIdentityAPI.Models.DTOs;
using CustomIdentityAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CustomIdentity2.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("[controller]")]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;

        private readonly SignInManager<IdentityUser> signInManager;
        private readonly TokenService tokenService;

        public AccountController(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
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

            if (model.EmailProp != null && !IsEmailValid(model.EmailProp))
            {
                return ValidationProblem("Email has incorrect format!");
            }

            if (ModelState.IsValid)
            {
                IdentityUser user = null;
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
            if (!IsEmailValid(model.EmailProp))
            {
                return ValidationProblem("Email has incorrect format!");
            }

            var user = new IdentityUser { UserName = model.LoginProp, Email = model.EmailProp };
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
                return GetUserDto(user);
            }

            return BadRequest("Problem with user registration.");
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<UserDataDto>> GetCurrentUser()
        {
            var user = await this.userManager.FindByEmailAsync(User.FindFirstValue(ClaimTypes.Email));
            return GetUserDto(user);
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

        private ActionResult<UserDataDto> GetUserDto(IdentityUser user)
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