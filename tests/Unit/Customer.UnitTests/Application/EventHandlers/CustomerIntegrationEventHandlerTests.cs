using PayNexa.Common.Persistence;
using PayNexa.Customers.Application.EventHandlers.Domain;
using PayNexa.Customers.Contracts.Events;
using PayNexa.Customers.Domain.CustomerAggregate.Events;
using PayNexa.Customers.Domain.CustomerAggregate.ValueObjects;
using PayNexa.Messaging.Abstractions;

namespace PayNexa.Customers.UnitTests.Application.EventHandlers;

public sealed class CustomerIntegrationEventHandlerTests
{
    private readonly IOutbox _outbox = Substitute.For<IOutbox>();

    [Fact]
    public async Task Registered_EnqueuesCreatedEventCarryingTheDomainEventIdentity()
    {
        var customer = CustomerTestData.RegisteredCustomer();
        var domainEvent = new CustomerRegisteredDomainEvent(customer, CustomerTestData.Now);

        await new CustomerIntegrationEventHandler(_outbox).HandleAsync(domainEvent, TestContext.Current.CancellationToken);

        _outbox.Received(1).Enqueue(Arg.Is<CustomerCreatedIntegrationEvent>(message =>
            message.EventId == domainEvent.EventId
            && message.Customer.Id == customer.Id.Value
            && message.Customer.Email == customer.Email.Value
            && message.PartitionKey == customer.Id.Value.ToString()));
    }

    [Fact]
    public async Task Suspended_EnqueuesUpdatedEventWithChangeType()
    {
        var customer = CustomerTestData.RegisteredCustomer();
        customer.Suspend(StatusReason.Create("Review"), CustomerTestData.Now);

        await new CustomerIntegrationEventHandler(_outbox).HandleAsync(new CustomerSuspendedDomainEvent(customer, CustomerTestData.Now), TestContext.Current.CancellationToken);

        _outbox.Received(1).Enqueue(Arg.Is<CustomerUpdatedIntegrationEvent>(message =>
            message.ChangeType == CustomerChangeTypes.Suspended && message.Customer.Status == "Suspended" && message.Customer.StatusReason == "Review"));
    }

    [Fact]
    public void IntegrationEvents_ShareTheCustomerTopic()
    {
        IntegrationEventMetadata.For<CustomerCreatedIntegrationEvent>().ShouldBe(new IntegrationEventMetadata(CustomerEventTypes.Created, 1, "paynexa.customer"));
        IntegrationEventMetadata.For<CustomerUpdatedIntegrationEvent>().ShouldBe(new IntegrationEventMetadata(CustomerEventTypes.Updated, 1, "paynexa.customer"));
    }
}
