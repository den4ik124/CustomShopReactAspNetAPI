using Microsoft.AspNetCore.Identity;
using OrmRepositoryUnitOfWork.Attributes;
using OrmRepositoryUnitOfWork.Enums;

[assembly: DomainModel]

namespace CustomIdentity.Models
{
    [Table("IdentityUsers")]
    public class CustomIdentityUser : IdentityUser<int>
    {
        [Column("Id", KeyType = KeyType.Primary)]
        public new int Id { get; set; }

        [Column("UserName", IsUniq = true)]
        public new string UserName { get; set; }

        [Column("Email", IsUniq = true)]
        public new string Email { get; set; }

        [Column("EmailNormalized", IsUniq = true)]
        public new string NormalizedEmail { get; set; }

        [Column("PasswordHash")]
        public new string PasswordHash { get; set; }
    }
}