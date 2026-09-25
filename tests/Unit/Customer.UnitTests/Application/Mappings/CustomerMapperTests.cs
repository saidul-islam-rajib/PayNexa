using PayNexa.Customers.Application.Mappings;
using PayNexa.Customers.Domain.CustomerAggregate.ValueObjects;

namespace PayNexa.Customers.UnitTests.Application.Mappings;

public sealed class CustomerMapperTests
{
    [Fact]
    public void ToResponse_MapsValueObjectsAndLifecycleFields()
    {
        var customer = CustomerTestData.RegisteredCustomer(address: CustomerTestData.DhakaAddress());
        customer.Suspend(StatusReason.Create("Review"), CustomerTestData.Now);

        var response = customer.ToResponse();

        response.Id.ShouldBe(customer.Id.Value);
        response.FirstName.ShouldBe("Saidul Islam");
        response.LastName.ShouldBe("Rajib");
        response.Email.ShouldBe("rajib@example.com");
        response.PhoneNumber.ShouldBe("+8801700000000");
        response.Address!.City.ShouldBe("Dhaka");
        response.Status.ShouldBe("Suspended");
        response.StatusReason.ShouldBe("Review");
        response.KycStatus.ShouldBe("Pending");
    }

    [Fact]
    public void ToSnapshot_IncludesVersionAndAudit()
    {
        var customer = CustomerTestData.RegisteredCustomer();

        var snapshot = customer.ToSnapshot();

        snapshot.Id.ShouldBe(customer.Id.Value);
        snapshot.Version.ShouldBe(customer.Version);
        snapshot.CreatedBy.ShouldBe(customer.CreatedBy);
        snapshot.Address.ShouldBeNull();
    }
}
