using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using UserDomainModel;

namespace CustomIdentityAPI.Services
{
    public class TokenService
    {
        private readonly UserManager<CustomIdentityUser> userManager;

        private IConfiguration config { get; }

        public TokenService(IConfiguration config, UserManager<CustomIdentityUser> userManager)
        {
            this.config = config;
            this.userManager = userManager;
        }

        public async Task<string> CreateToken(CustomIdentityUser user)
        {
            //new
            var roles = await this.userManager.GetRolesAsync(user);
            //new

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
            };

            //new
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            //new

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.config["TokenKey"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
            //return await Task.Run(() => tokenHandler.WriteToken(token));
        }
    }
}