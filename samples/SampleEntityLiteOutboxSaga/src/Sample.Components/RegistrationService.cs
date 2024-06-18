namespace Sample.Components;

using Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;

public class RegistrationService :
    IRegistrationService
{
    readonly RegistrationDataService _ds;
    readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<RegistrationService> logger;

    public RegistrationService(RegistrationDataService ds, IPublishEndpoint publishEndpoint, ILogger<RegistrationService> logger)
    {
        _ds = ds;
        _publishEndpoint = publishEndpoint;
        this.logger = logger;
    }

    public async Task<Registration> SubmitRegistration(string eventId, string memberId, decimal payment)
    {
        var registration = new Registration
        {
            RegistrationId = NewId.NextGuid(),
            RegistrationDate = DateTime.UtcNow,
            MemberId = memberId,
            EventId = eventId,
            Payment = payment,
        };
        _ds.BeginTransaction();
        try
        {
            await SubmitRegistration(registration);
            _ds.Commit();
            return registration;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error submitting registration");
            try
            {
                if (_ds.IsActiveTransaction) _ds.Rollback();
            }
            catch (Exception rollbackException)
            {
                logger.LogError(rollbackException, "Error rolling back transaction");
            }
            throw;
        }
    }

    private async Task SubmitRegistration(Registration registration)
    {
        try
        {
            await _ds.RegistrationRepository.InsertAsync(registration);
            await _publishEndpoint.Publish(new RegistrationSubmitted
            {
                RegistrationId = registration.RegistrationId,
                RegistrationDate = registration.RegistrationDate,
                MemberId = registration.MemberId,
                EventId = registration.EventId,
                Payment = registration.Payment
            });

        }
        catch (SqlException sqlException) when (sqlException.Number == 2627 || sqlException.Number == 2601)
        {
            throw new DuplicateRegistrationException("Duplicate registration", sqlException);
        }
    }
}
