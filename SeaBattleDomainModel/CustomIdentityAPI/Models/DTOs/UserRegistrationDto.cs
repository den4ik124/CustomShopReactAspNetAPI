using System.ComponentModel.DataAnnotations;

namespace CustomIdentityAPI.Models.DTOs
{
    public class UserRegistrationDto
    {
        [Required, MaxLength(20)]
        public string LoginProp { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Required, MaxLength(50)]
        public string EmailProp { get; set; }

        [DataType(DataType.Password), Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }
    }
}