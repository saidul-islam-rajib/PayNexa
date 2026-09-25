using PayNexa.Customers.Application.Commands.Common;
using PayNexa.Customers.Contracts.Common;

namespace PayNexa.Customers.Application.Commands.ChangeCustomerAddress;

public sealed record ChangeCustomerAddressCommand(Guid CustomerId, AddressDto Address) : ICustomerCommand;
