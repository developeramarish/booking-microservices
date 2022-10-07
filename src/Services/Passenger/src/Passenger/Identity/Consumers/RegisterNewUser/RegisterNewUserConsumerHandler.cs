using Ardalis.GuardClauses;
using BuildingBlocks.Contracts.EventBus.Messages;
using BuildingBlocks.Core;
using BuildingBlocks.Core.Event;
using BuildingBlocks.IdsGenerator;
using BuildingBlocks.Web;
using Humanizer;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Passenger.Data;
using Passenger.Passengers.Events.Domain;

namespace Passenger.Identity.Consumers.RegisterNewUser;

public class RegisterNewUserConsumerHandler : IConsumer<UserCreated>
{
    private readonly PassengerDbContext _passengerDbContext;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly ILogger<RegisterNewUserConsumerHandler> _logger;
    private readonly AppOptions _options;

    public RegisterNewUserConsumerHandler(PassengerDbContext passengerDbContext,
        IEventDispatcher eventDispatcher,
        ILogger<RegisterNewUserConsumerHandler> logger,
        IOptions<AppOptions> options)
    {
        _passengerDbContext = passengerDbContext;
        _eventDispatcher = eventDispatcher;
        _logger = logger;
        _options = options.Value;
    }

    public async Task Consume(ConsumeContext<UserCreated> context)
    {
        Guard.Against.Null(context.Message, nameof(UserCreated));

        _logger.LogInformation($"consumer for {nameof(UserCreated).Underscore()} in {_options.Name}");

        var passengerExist =
            await _passengerDbContext.Passengers.AnyAsync(x => x.PassportNumber == context.Message.PassportNumber);

        if (passengerExist)
            return;

        var passenger = Passengers.Models.Passenger.Create(SnowFlakIdGenerator.NewId(), context.Message.Name,
            context.Message.PassportNumber);

        await _passengerDbContext.AddAsync(passenger);

        await _passengerDbContext.SaveChangesAsync();

        await _eventDispatcher.SendAsync(
            new PassengerCreatedDomainEvent(passenger.Id, passenger.Name, passenger.PassportNumber),
            typeof(IInternalCommand));
    }
}