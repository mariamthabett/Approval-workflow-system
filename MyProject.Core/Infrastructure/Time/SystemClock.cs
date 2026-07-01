using MyProject.Core.Application.Abstractions;

namespace MyProject.Core.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
