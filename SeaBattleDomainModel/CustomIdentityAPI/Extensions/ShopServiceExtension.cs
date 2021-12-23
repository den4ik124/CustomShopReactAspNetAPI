using CustomIdentityAPI.DAL;
using CustomIdentityAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace CustomIdentityAPI.Extensions
{
    public static class ShopServiceExtension
    {
        public static IServiceCollection AddShopServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<ShopDbContext>(opt =>
            {
                opt.UseSqlServer(config.GetConnectionString("ShopDb"));
            });

            services.AddScoped<TokenService>();

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opt =>
                {
                    opt.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = key,
                        ValidateIssuer = false,
                        ValidateAudience = false,
                    };
                }
                ).AddCookie();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequiredLength = 3;

                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(3);
                options.Lockout.AllowedForNewUsers = true;
            });

            services.AddCors(opt =>
            {
                opt.AddPolicy("CorsPolicy", policy =>
                {
                    policy.AllowAnyMethod().AllowAnyHeader().WithOrigins("http://localhost:3000"); //.WithOrigins("http://localhost:3000");
                });
            });

            return services;
        }

        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
        {
            return services;
        }
    }
}