using CustomIdentityAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Persistence;
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

            
            services.AddCors(opt =>
            {
                opt.AddPolicy("CorsPolicy", policy =>
                {
                    policy.AllowAnyMethod().AllowAnyHeader().WithOrigins("http://localhost:3000"); //.WithOrigins("http://localhost:3000");
                });
            });

            return services;
        }
    }
}