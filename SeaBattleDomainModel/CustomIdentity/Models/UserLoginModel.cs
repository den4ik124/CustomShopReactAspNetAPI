using System.ComponentModel.DataAnnotations;

namespace CustomIdentity.Models
{
    public class UserLoginModel
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