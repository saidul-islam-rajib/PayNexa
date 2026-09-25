using PayNexa.Common.Persistence;
using PayNexa.Customers.Application.Commands.ChangeCustomerEmail;
using PayNexa.Customers.Application.Commands.SuspendCustomer;
using PayNexa.Customers.Application.Commands.VerifyCustomerKyc;
using PayNexa.Customers.Application.Interfaces;
using PayNexa.Customers.Domain.Common.Errors;
using PayNexa.Customers.Domain.CustomerAggregate;
using PayNexa.Customers.Domain.CustomerAggregate.Events;
using PayNexa.Customers.Domain.CustomerAggregate.ValueObjects;
using PayNexa.SharedKernel.Domain;

namespace PayNexa.Customers.UnitTests.Application.Commands;

public sealed class CustomerCommandHandlerTests
{
    private readonly ICustomerRepository _customers = Substitute.For<ICustomerRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Suspend_ExistingCustomer_AppliesBehaviourAndSaves()
    {
        var customer = Existing();

        var result = await SuspendHandler().Handle(new SuspendCustomerCommand(customer.Id.Value, "Fraud review"), TestContext.Current.CancellationToken);

        result.Value.Status.ShouldBe("Suspended");
        result.Value.StatusReason.ShouldBe("Fraud review");
        customer.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<CustomerSuspendedDomainEvent>();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Suspend_UnknownCustomer_ReturnsNotFound()
    {
        var result = await SuspendHandler().Handle(new SuspendCustomerCommand(Guid.CreateVersion7(), "Fraud review"), TestContext.Current.CancellationToken);

        result.Error!.Code.ShouldBe(CustomerErrorCodes.NotFound);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Suspend_ConcurrentModification_ReturnsConflict()
    {
        Existing();
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromException(new ConcurrencyConflictException("stale")));

        var result = await SuspendHandler().Handle(new SuspendCustomerCommand(Guid.CreateVersion7(), "Fraud review"), TestContext.Current.CancellationToken);

        result.Error.ShouldBe(Errors.Customer.ConcurrentModification);
    }

    [Fact]
    public async Task VerifyKyc_WithoutAddress_SurfacesTheDomainRule()
    {
        var customer = Existing();
        var handler = new VerifyCustomerKycCommandHandler(_customers, _unitOfWork, CustomerTestData.TimeProvider(), new FakeLogger<VerifyCustomerKycCommandHandler>());

        (await Should.ThrowAsync<DomainException>(async () =>
                await handler.Handle(new VerifyCustomerKycCommand(customer.Id.Value), TestContext.Current.CancellationToken)))
            .Error.Code.ShouldBe(CustomerErrorCodes.AddressRequiredForKyc);
    }

    [Fact]
    public async Task ChangeEmail_ToEmailOwnedByAnotherCustomer_ReturnsConflict()
    {
        var customer = Existing();
        _customers.EmailExistsAsync(Email.Create("taken@example.com"), Arg.Any<CancellationToken>()).Returns(true);

        var result = await EmailHandler().Handle(new ChangeCustomerEmailCommand(customer.Id.Value, "taken@example.com"), TestContext.Current.CancellationToken);

        result.Error.ShouldBe(Errors.Customer.EmailAlreadyRegistered);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ChangeEmail_ToOwnEmail_DoesNotRequireUniquenessCheck()
    {
        var customer = Existing();

        var result = await EmailHandler().Handle(new ChangeCustomerEmailCommand(customer.Id.Value, "RAJIB@example.com"), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await _customers.DidNotReceive().EmailExistsAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>());
    }

    private Customer Existing()
    {
        var customer = CustomerTestData.RegisteredCustomer();
        _customers.GetByIdAsync(Arg.Any<CustomerId>(), Arg.Any<CancellationToken>()).Returns(customer);
        return customer;
    }

    private SuspendCustomerCommandHandler SuspendHandler() =>
        new(_customers, _unitOfWork, CustomerTestData.TimeProvider(), new FakeLogger<SuspendCustomerCommandHandler>());

    private ChangeCustomerEmailCommandHandler EmailHandler() =>
        new(_customers, _unitOfWork, CustomerTestData.TimeProvider(), new FakeLogger<ChangeCustomerEmailCommandHandler>());
}
