using PayNexa.Customers.Contracts.Common;

namespace PayNexa.Customers.Contracts.Requests;

public sealed record ChangeCustomerAddressRequest(AddressDto Address);
