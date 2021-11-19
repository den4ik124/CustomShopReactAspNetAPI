using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_Repos_UoW.Enums
{
    [Flags]
    public enum ReadWriteOption
    {
        Read = 1,
        Write = 2,
    }
}