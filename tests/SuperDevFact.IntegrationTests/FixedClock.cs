using SuperDevFact.Application.Abstractions;

namespace SuperDevFact.IntegrationTests;

internal sealed class FixedClock(DateOnly today) : IClock
{
    public DateTimeOffset UtcNow { get; } = new(today.ToDateTime(TimeOnly.MinValue));

    public DateOnly Today { get; } = today;
}
