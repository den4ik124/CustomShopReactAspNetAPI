using System;

namespace OrmRepositoryUnitOfWork.Enums
{
    [Flags]
    public enum ReadWriteOption
    {
        Read = 1,
        Write = 2,
    }
}