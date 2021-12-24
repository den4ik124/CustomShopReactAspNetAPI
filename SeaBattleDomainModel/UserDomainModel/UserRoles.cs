using System.ComponentModel.DataAnnotations.Schema;

namespace UserDomainModel
{
    [NotMapped]
    public class UserRoles
    {
        public string UserId { get; set; }

        public CustomIdentityUser User { get; set; }

        public string RoleId { get; set; }

        public CustomRoles Role { get; set; }
    }
}