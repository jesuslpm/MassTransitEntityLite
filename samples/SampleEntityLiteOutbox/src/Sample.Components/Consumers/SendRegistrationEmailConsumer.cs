namespace Sample.Components.Consumers;

using Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;


public class SendRegistrationEmailConsumer :
    IConsumer<SendRegistrationEmail>
{
    readonly ILogger<SendRegistrationEmailConsumer> _logger;

    public SendRegistrationEmailConsumer(ILogger<SendRegistrationEmailConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SendRegistrationEmail> context)
    {
        _logger.LogInformation("SendRegistrationEmailConsumer: Sending email to Member {MemberId} that they registered for event {EventId} on {RegistrationDate}", context.Message.MemberId,
            context.Message.EventId, context.Message.RegistrationDate);

        await Task.CompletedTask;
    }
}