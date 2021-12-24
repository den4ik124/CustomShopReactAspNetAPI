using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UserDomainModel;

namespace Persistence
{
    public class UserDbContext : IdentityDbContext
    {
        private readonly IConfiguration config;

        public UserDbContext(DbContextOptions<UserDbContext> options, IConfiguration config) : base(options)
        {
            Database.EnsureCreated();
            this.config = config;
        }

        public DbSet<CustomIdentityUser> CustomUsers { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer(this.config.GetConnectionString("UsersDb"));
        //}
    }
}