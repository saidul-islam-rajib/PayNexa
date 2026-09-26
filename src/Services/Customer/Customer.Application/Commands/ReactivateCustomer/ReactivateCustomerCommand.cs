using PayNexa.Customers.Application.Commands.Common;

namespace PayNexa.Customers.Application.Commands.ReactivateCustomer;

public sealed record ReactivateCustomerCommand(Guid CustomerId) : ICustomerCommand;
