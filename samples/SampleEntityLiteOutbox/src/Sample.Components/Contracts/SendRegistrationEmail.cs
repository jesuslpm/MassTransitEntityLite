using MassTransit;

namespace Sample.Components.Contracts;

public record SendRegistrationEmail: CorrelatedBy<Guid> 
{
    public Guid RegistrationId { get; init; }
    public DateTime RegistrationDate { get; init; }
    public string MemberId { get; init; } = null!;
    public string EventId { get; init; } = null!;
    public Guid CorrelationId => RegistrationId;
}