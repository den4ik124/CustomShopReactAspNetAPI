using Microsoft.AspNetCore.Identity;

namespace UserDomainModel
{
    public class CustomRoles : IdentityRole
    {
        // This class was created for adding new user features in the future if such appears

        public CustomRoles()
        {

        }
        public CustomRoles(string roleName): base(roleName)
        {

        }
        public string Description { get; set; }
    }
}