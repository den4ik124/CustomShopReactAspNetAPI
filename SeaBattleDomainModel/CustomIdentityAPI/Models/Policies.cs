using System;

namespace CustomIdentityAPI.Models
{
    [Flags]
    public enum Policies
    {
        AdminAccess = 1,
        ManagerAccess = 2,
        CustomerAccess = 4,
    }
}
