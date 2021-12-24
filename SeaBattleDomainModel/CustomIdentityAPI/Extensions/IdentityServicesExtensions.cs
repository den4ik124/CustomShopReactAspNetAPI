using CustomIdentityAPI.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Persistence;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using CustomIdentityAPI.Services;
using Microsoft.EntityFrameworkCore;
using UserDomainModel;

namespace CustomIdentityAPI.Extensions
{
    public static class IdentityServicesExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services,
            IConfiguration config)
        {
            services.AddDbContext<UserDbContext>(opt =>
            {
                opt.UseSqlServer(config.GetConnectionString("UsersDb"));
            });

            services.AddIdentityCore<CustomIdentityUser>(opt =>
            {
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequireUppercase = false;
            })
                .AddRoles<CustomRoles>()
                //.AddRoleStore<UserDbContext>()
                .AddRoleManager<RoleManager<CustomRoles>>()
                .AddEntityFrameworkStores<UserDbContext>()
                .AddSignInManager<SignInManager<CustomIdentityUser>>();

            services.AddAuthentication(opt => { });

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opt =>
                {
                    opt.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = key,
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });
            services.AddScoped<TokenService>();
            return services;
        }
    }
}