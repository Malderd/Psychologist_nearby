using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Psychologist_nearby.Tests;

public sealed class LoginDatabaseFaultInjector
    : SaveChangesInterceptor
{
    public bool Enabled { get; set; } = false;

    public int SaveAttempts { get; private set; }

    public override ValueTask<InterceptionResult<int>>
        SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
    {
        if (Enabled)
        {
            SaveAttempts++;

            if (SaveAttempts == 2)
            {
                throw new DbUpdateException(
                    "FI-03: Database failure during code cancellation");
            }
        }

        return base.SavingChangesAsync(
            eventData, result, cancellationToken);
    }
}