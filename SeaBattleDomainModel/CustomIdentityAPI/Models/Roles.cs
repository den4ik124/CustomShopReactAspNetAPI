using System;

namespace CustomIdentityAPI.Models
{
    [Flags]
    public enum Roles
    {
        Admin = 1,
        Manager = 2,
        Customer = 4,
    }
}
