using MassTransit;

namespace Sample.Components.Consumers
{
    public class RegistrationConsumerDefinition :
        ConsumerDefinition<RegistrationConsumer>
    {
        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
            IConsumerConfigurator<RegistrationConsumer> consumerConfigurator, IRegistrationContext context)
        {
            endpointConfigurator.UseMessageRetry(r => r.Intervals(10, 50, 100, 1000, 2000));
            endpointConfigurator.UseEntityLiteOutbox<RegistrationDataService>(context);
        }
    }
}
