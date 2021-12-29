using Application.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence;

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

            services.AddMediatR(typeof(List.Handler).Assembly);

            return services;
        }
    }
}