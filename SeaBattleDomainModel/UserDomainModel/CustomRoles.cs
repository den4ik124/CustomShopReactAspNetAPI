using Microsoft.AspNetCore.Identity;

namespace UserDomainModel
{
    public class CustomRoles : IdentityRole
    {
        // This class was created for adding new user features in the future if such appears

        public string Description { get; set; }
    }
}