using PayNexa.Common.Persistence;
using PayNexa.Customers.Application.Commands.UpdateCustomer;
using PayNexa.Customers.Application.Errors;
using PayNexa.Customers.Application.Interfaces;
using PayNexa.Customers.Contracts.Events;

namespace PayNexa.Customers.UnitTests.Application.Commands;

public sealed class UpdateCustomerCommandHandlerTests
{
    private readonly ICustomerRepository _customers = Substitute.For<ICustomerRepository>();
    private readonly IOutbox _outbox = Substitute.For<IOutbox>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateCustomerCommandHandler _handler;

    public UpdateCustomerCommandHandlerTests() =>
        _handler = new UpdateCustomerCommandHandler(_customers, _outbox, _unitOfWork, CustomerTestData.TimeProvider(), new FakeLogger<UpdateCustomerCommandHandler>());

    [Fact]
    public async Task Handle_ExistingCustomer_UpdatesAndEnqueuesUpdatedEventWithNewVersion()
    {
        var customer = CustomerTestData.Customer();
        _customers.GetByIdAsync(customer.Id, Arg.Any<CancellationToken>()).Returns(customer);

        var result = await _handler.Handle(new UpdateCustomerCommand(customer.Id, "Saidul", "Islam", "+8801711111111"), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.LastName.ShouldBe("Islam");
        _outbox.Received(1).Enqueue(Arg.Is<CustomerUpdatedIntegrationEvent>(message =>
            message.CustomerId == customer.Id && message.Version == 2 && message.PhoneNumber == "+8801711111111"));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownCustomer_ReturnsNotFound()
    {
        var id = Guid.CreateVersion7();

        var result = await _handler.Handle(new UpdateCustomerCommand(id, "Saidul", "Rajib", "+8801700000000"), TestContext.Current.CancellationToken);

        result.Error!.Code.ShouldBe("Customer.NotFound");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConcurrentModification_ReturnsConflict()
    {
        var customer = CustomerTestData.Customer();
        _customers.GetByIdAsync(customer.Id, Arg.Any<CancellationToken>()).Returns(customer);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new ConcurrencyConflictException("stale")));

        var result = await _handler.Handle(new UpdateCustomerCommand(customer.Id, "Saidul", "Rajib", "+8801700000000"), TestContext.Current.CancellationToken);

        result.Error.ShouldBe(CustomerErrors.ConcurrentModification);
    }
}
