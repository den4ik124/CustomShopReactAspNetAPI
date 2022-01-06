using CustomIdentityAPI.Models;
using CustomIdentityAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Persistence;
using System.Text;
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

            //services.AddIdentity<CustomIdentityUser, CustomRoles>(opt =>
            services.AddIdentityCore<CustomIdentityUser>(opt =>
            {
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequireUppercase = false;
                //opt.ClaimsIdentity.RoleClaimType = "role";
                //opt.ClaimsIdentity.EmailClaimType = "emailaddress";
            })
                .AddRoles<CustomRoles>()
                .AddRoleManager<RoleManager<CustomRoles>>()
                .AddEntityFrameworkStores<UserDbContext>()
                .AddSignInManager<SignInManager<CustomIdentityUser>>();

           //services.AddAuthentication(opt => { });

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opt =>
                {
                    opt.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = key,
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        
                        //NameClaimType = "name",
                        //RoleClaimType =  "role",
                    };
                });

            services.AddAuthorization(opt =>
            {
                opt.AddPolicy(nameof(Policies.AdminAccess), policy => policy.RequireRole(nameof(Roles.Admin)));
                opt.AddPolicy(nameof(Policies.ManagerAccess), policy => policy.RequireAssertion(context =>
                                        context.User.IsInRole(nameof(Roles.Admin)) ||
                                        context.User.IsInRole(nameof(Roles.Manager))));
                opt.AddPolicy(nameof(Policies.CustomerAccess), policy => policy.RequireAssertion(context =>
                                        context.User.IsInRole(nameof(Roles.Admin)) ||
                                        context.User.IsInRole(nameof(Roles.Manager)) ||
                                        context.User.IsInRole(nameof(Roles.Customer))));
            });

            services.AddScoped<TokenService>();
            return services;
        }
    }
}