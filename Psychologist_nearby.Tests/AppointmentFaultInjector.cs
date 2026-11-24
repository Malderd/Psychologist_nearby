using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Threading;
using System.Threading.Tasks;

public enum AppointmentFault
{
    None,
    DatabaseError,
    Timeout
}

public sealed class AppointmentFaultInjector : SaveChangesInterceptor
{
    public AppointmentFault Mode { get; set; } = AppointmentFault.None;

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        switch (Mode)
        {
            case AppointmentFault.DatabaseError:
                throw new DbUpdateException(
                    "FI-01: Simulated database error");

            case AppointmentFault.Timeout:
                throw new TimeoutException(
                    "FI-02: Simulated database timeout");

            default:
                return base.SavingChangesAsync(
                    eventData, result, cancellationToken);
        }
    }
}