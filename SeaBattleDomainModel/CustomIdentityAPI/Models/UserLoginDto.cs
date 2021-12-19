using System.ComponentModel.DataAnnotations;

namespace CustomIdentityAPI.Models
{
    public class UserLoginDto
    {
        [MaxLength(20)]
        public string LoginProp { get; set; }

        [MaxLength(50)]
        [EmailAddress]
        public string EmailProp { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        public string ReturnUrl { get; set; }
    }
}