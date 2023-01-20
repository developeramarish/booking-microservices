﻿namespace Unit.Test.Flight.Features.Domain;

using System.Linq;
using FluentAssertions;
using global::Flight.Flights.Features.UpdateFlight.Events.V1;
using Unit.Test.Common;
using Unit.Test.Fakes;
using Xunit;

[Collection(nameof(UnitTestFixture))]
public class UpdateFlightTests
{
    [Fact]
    public void can_update_valid_flight()
    {
        // Arrange
        var fakeFlight = FakeFlightCreate.Generate();

        // Act
        FakeFlightUpdate.Generate(fakeFlight);

        // Assert
        fakeFlight.ArriveAirportId.Should().Be(3);
        fakeFlight.AircraftId.Should().Be(3);
    }

    [Fact]
    public void queue_domain_event_on_update()
    {
        // Arrange
        var fakeFlight = FakeFlightCreate.Generate();
        fakeFlight.ClearDomainEvents();

        // Act
        FakeFlightUpdate.Generate(fakeFlight);

        // Assert
        fakeFlight.DomainEvents.Count.Should().Be(1);
        fakeFlight.DomainEvents.FirstOrDefault().Should().BeOfType(typeof(FlightUpdatedDomainEvent));
    }
}