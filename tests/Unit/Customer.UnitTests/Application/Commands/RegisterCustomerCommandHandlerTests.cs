using Microsoft.Extensions.Options;
using PayNexa.Common.Persistence;
using PayNexa.Customers.Application.Commands.RegisterCustomer;
using PayNexa.Customers.Application.Interfaces;
using PayNexa.Customers.Application.Options;
using PayNexa.Customers.Contracts.Common;
using PayNexa.Customers.Domain.Common.Errors;
using PayNexa.Customers.Domain.CustomerAggregate;
using PayNexa.Customers.Domain.CustomerAggregate.Events;
using PayNexa.Customers.Domain.CustomerAggregate.ValueObjects;

namespace PayNexa.Customers.UnitTests.Application.Commands;

public sealed class RegisterCustomerCommandHandlerTests
{
    private readonly ICustomerRepository _customers = Substitute.For<ICustomerRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly RegisterCustomerCommandHandler _handler;

    public RegisterCustomerCommandHandlerTests() =>
        _handler = new RegisterCustomerCommandHandler(
            _customers,
            _unitOfWork,
            CustomerTestData.TimeProvider(),
            Options.Create(new CustomerOptions()),
            new FakeLogger<RegisterCustomerCommandHandler>());

    [Fact]
    public async Task Handle_NewEmailWithAddress_AddsCustomerRaisingOnlyTheRegisteredEvent()
    {
        Customer? added = null;
        _customers.Add(Arg.Do<Customer>(customer => added = customer));

        var result = await _handler.Handle(Command(), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Email.ShouldBe("saidul.is.rajib@gmail.com");
        result.Value.Address!.CountryCode.ShouldBe("BD");
        result.Value.KycStatus.ShouldBe("Pending");
        added.ShouldNotBeNull();
        added.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<CustomerRegisteredDomainEvent>();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmailAlreadyRegistered_ReturnsConflictWithoutPersisting()
    {
        _customers.EmailExistsAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>()).Returns(true);

        var result = await _handler.Handle(Command(), TestContext.Current.CancellationToken);

        result.Error.ShouldBe(Errors.Customer.EmailAlreadyRegistered);
        _customers.DidNotReceive().Add(Arg.Any<Customer>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConcurrentRegistrationWinsUniqueIndex_ReturnsConflict()
    {
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new UniqueConstraintViolationException("duplicate")));

        var result = await _handler.Handle(Command(), TestContext.Current.CancellationToken);

        result.Error.ShouldBe(Errors.Customer.EmailAlreadyRegistered);
    }

    [Fact]
    public async Task Handle_WriteStoreUnavailable_PropagatesException()
    {
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new TimeoutException("sql timeout")));

        await Should.ThrowAsync<TimeoutException>(async () => await _handler.Handle(Command(), TestContext.Current.CancellationToken));
    }

    private static RegisterCustomerCommand Command() => new(
        "Saidul Islam",
        "Rajib",
        "Saidul.Is.Rajib@gmail.com",
        "+8801700000000",
        new DateOnly(1995, 5, 20),
        new AddressDto("Mirpur Road", null, "Dhaka", "Dhaka", "1207", "bd"));
}
