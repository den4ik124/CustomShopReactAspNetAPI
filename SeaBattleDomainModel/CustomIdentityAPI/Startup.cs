using CustomIdentityAPI.Extensions;
using CustomIdentityAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using UserDomainModel;

namespace CustomIdentityAPI
{
    public class Startup
    {
        private IConfiguration config;

        public Startup(IConfiguration configuration)
        {
            this.config = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(opt =>
            {
                var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                opt.Filters.Add(new AuthorizeFilter(policy));
            });

            services.AddShopServices(this.config);
            services.AddIdentityServices(this.config);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IServiceProvider serviceProvider)
        {
            app.UseRouting();

            app.UseCors("CorsPolicy");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            CreateRoles(serviceProvider);
        }


        private void CreateRoles(IServiceProvider serviceProvider)
        {
            //TODO : test message new
            var roleManager = serviceProvider.GetRequiredService<RoleManager<CustomRoles>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<CustomIdentityUser>>();
            Task<IdentityResult> roleResult;
            string login = "admin";
            string email = "admin@admin.com";

            //Check that there is an Administrator role and create if not
            Task<bool> hasAdminRole = roleManager.RoleExistsAsync(nameof(Roles.Admin));
            hasAdminRole.Wait();

            if (!hasAdminRole.Result)
            {
                roleResult = roleManager.CreateAsync(new CustomRoles(nameof(Roles.Admin)));
                roleResult.Wait();
            }

            //Check if the admin user exists and create it if not
            //Add to the Administrator role

            Task<CustomIdentityUser> testUser = userManager.FindByEmailAsync(email);
            testUser.Wait();

            if (testUser.Result == null)
            {
                CustomIdentityUser administrator = new CustomIdentityUser();
                administrator.Email = email;
                administrator.UserName = login;

                Task<IdentityResult> newUser = userManager.CreateAsync( administrator, "admin123");
                newUser.Wait();

                if (newUser.Result.Succeeded)
                {
                    Task<IdentityResult> newUserRole = userManager.AddToRoleAsync(administrator, nameof(Roles.Admin));
                    newUserRole.Wait();
                }
            }

        }
    }
}