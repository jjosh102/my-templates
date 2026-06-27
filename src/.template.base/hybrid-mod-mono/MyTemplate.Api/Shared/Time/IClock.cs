namespace MyTemplate.Api.Shared.Time;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
