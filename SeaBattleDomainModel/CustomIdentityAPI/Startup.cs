using CustomIdentityAPI.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

            //services.AddDbContext<SeaBattleDbContext>(opt =>
            //{
            //    opt.UseSqlServer(Configuration.GetConnectionString("SeaBattleDB"));
            //});
            //services.AddDBServices();

            //services.AddTransient<IUnitOfWork, UnitOfWork>((service) => new UnitOfWork(Configuration.GetConnectionString("CustomConnection"), null));
            //services.AddTransient<IUserStore<CustomIdentityUser>, CustomUserStore>();
            //services.AddTransient<IShipData, ShipData>((service) => new ShipData(new UnitOfWork(Configuration.GetConnectionString("ShipsDBConnection"), null)));
            //services.AddIdentityCore<CustomIdentityUser>(o =>
            //{
            //    o.Stores.MaxLengthForKeys = 128;
            //    o.SignIn.RequireConfirmedAccount = false;
            //})
            //.AddDefaultTokenProviders()
            //.AddSignInManager<SignInManager<CustomIdentityUser>>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app/*, IWebHostEnvironment env*/)
        {
            app.UseRouting();

            app.UseCors("CorsPolicy");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}