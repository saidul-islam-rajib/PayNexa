using PayNexa.Customers.Contracts.Common;
using PayNexa.Customers.Domain.CustomerAggregate.ValueObjects;

namespace PayNexa.Customers.Application.Mappings;

public static class CustomerValueObjectMapper
{
    public static Address ToAddress(this AddressDto address) =>
        Address.Create(address.Line1, address.Line2, address.City, address.State, address.PostalCode, address.CountryCode);
}
