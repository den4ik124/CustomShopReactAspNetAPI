using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CustomIdentityAPI.Models.DTOs
{
    public class UserDataDto
    {
        public string LoginProp { get; set; }

        [EmailAddress]
        public string EmailProp { get; set; }

        public string Token { get; set; }

        public IList<string> Roles { get; set; }
    }
}