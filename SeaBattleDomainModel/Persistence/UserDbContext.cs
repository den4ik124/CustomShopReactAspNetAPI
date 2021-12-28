using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserDomainModel;

namespace Persistence
{
    public class UserDbContext : IdentityDbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<CustomIdentityUser> CustomUsers { get; set; }
        public DbSet<CustomRoles> CustomRoles { get; set; }

        //protected override void OnModelCreating(ModelBuilder builder)
        //{
        //    base.OnModelCreating(builder);

        //    builder.Entity<UserRoles>(item => item.HasKey(ur => new { ur.UserId, ur.RoleId }));

        //    builder.Entity<UserRoles>()
        //        .HasOne(u => u.User)
        //        .WithMany(r => r.Roles)
        //        .HasForeignKey(op => op.UserId);

        //    builder.Entity<UserRoles>()
        //        .HasOne(o => o.Role)
        //        .WithMany(p => p.Users)
        //        .HasForeignKey(op => op.RoleId);
        //}
    }
}