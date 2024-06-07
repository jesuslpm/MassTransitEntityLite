using MassTransit;
using Microsoft.Extensions.Logging;
using Sample.Components.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Components.Consumers
{
    public class RegistrationConsumer :
        IConsumer<RegistrationSubmitted>
    {

        private readonly ILogger<RegistrationConsumer> logger;
        private readonly RegistrationDataService ds;

        public RegistrationConsumer(ILogger<RegistrationConsumer> logger, RegistrationDataService ds)
        {
            this.logger = logger;
            this.ds = ds;
        }

        public async Task Consume(ConsumeContext<RegistrationSubmitted> context)
        {
            if (ds.IsActiveTransaction == false)
            {
                throw new InvalidOperationException("Transaction must be active to consume RegistrationSubmitted message in RegistrationConsumer");
            }
            try
            {
                var registration = new Registration
                {
                    RegistrationId = context.Message.RegistrationId,
                    CurrentState = RegistrationStates.Registered
                };
                await ds.RegistrationRepository.UpdateAsync(registration, RegistrationFields.CurrentState);
                await context.Publish(new SendRegistrationEmail
                {
                    EventId = context.Message.EventId,
                    MemberId = context.Message.MemberId,
                    RegistrationDate = context.Message.RegistrationDate,
                    RegistrationId = context.Message.RegistrationId
                });
                logger.LogInformation("RegistrationConsumer: Registration State of {RegistrationId} Updated to {CurrentState}", context.Message.RegistrationId, registration.CurrentState);

            }
            catch (Exception ex) {                 
                logger.LogError(ex, "Failed to consume RegistrationSubmitted in RegistrationConsumer");
                throw;
            }    
        }
    }
}
