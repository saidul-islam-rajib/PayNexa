using PayNexa.Common.Persistence;
using PayNexa.Customers.Application.Commands.CreateCustomer;
using PayNexa.Customers.Application.Errors;
using PayNexa.Customers.Application.Interfaces;
using PayNexa.Customers.Contracts.Events;
using PayNexa.Customers.Domain.Entities;
using PayNexa.Customers.Domain.ValueObjects;

namespace PayNexa.Customers.UnitTests.Application.Commands;

public sealed class CreateCustomerCommandHandlerTests
{
    private readonly ICustomerRepository _customers = Substitute.For<ICustomerRepository>();
    private readonly IOutbox _outbox = Substitute.For<IOutbox>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateCustomerCommandHandler _handler;

    public CreateCustomerCommandHandlerTests() =>
        _handler = new CreateCustomerCommandHandler(_customers, _outbox, _unitOfWork, CustomerTestData.TimeProvider(), new FakeLogger<CreateCustomerCommandHandler>());

    [Fact]
    public async Task Handle_NewEmail_PersistsCustomerAndEnqueuesCreatedEventInSameUnitOfWork()
    {
        var result = await _handler.Handle(Command(), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Email.ShouldBe("saidul.is.rajib@gmail.com");
        result.Value.Status.ShouldBe("Active");

        _customers.Received(1).Add(Arg.Is<Customer>(customer => customer.Id == result.Value.Id));
        _outbox.Received(1).Enqueue(Arg.Is<CustomerCreatedIntegrationEvent>(message =>
            message.CustomerId == result.Value.Id && message.Version == 1));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmailAlreadyRegistered_ReturnsConflictWithoutPersisting()
    {
        _customers.EmailExistsAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>()).Returns(true);

        var result = await _handler.Handle(Command(), TestContext.Current.CancellationToken);

        result.Error.ShouldBe(CustomerErrors.EmailAlreadyRegistered);
        _customers.DidNotReceive().Add(Arg.Any<Customer>());
        _outbox.DidNotReceiveWithAnyArgs().Enqueue<CustomerCreatedIntegrationEvent>(default!);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConcurrentRegistrationWinsUniqueIndex_ReturnsConflict()
    {
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new UniqueConstraintViolationException("duplicate")));

        var result = await _handler.Handle(Command(), TestContext.Current.CancellationToken);

        result.Error.ShouldBe(CustomerErrors.EmailAlreadyRegistered);
    }

    [Fact]
    public async Task Handle_WriteStoreUnavailable_PropagatesException()
    {
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new TimeoutException("sql timeout")));

        await Should.ThrowAsync<TimeoutException>(async () => await _handler.Handle(Command(), TestContext.Current.CancellationToken));
    }

    private static CreateCustomerCommand Command() =>
        new("Saidul Islam", "Rajib", "Saidul.Is.Rajib@gmail.com", "+8801700000000", new DateOnly(1995, 5, 20));
}
