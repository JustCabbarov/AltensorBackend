using System;

namespace Altensorcrm.Application.Exceptions;

public class TenantSuspendedException : Exception
{
    public TenantSuspendedException(string message) : base(message) { }
}
