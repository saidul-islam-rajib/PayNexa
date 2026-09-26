using PayNexa.Customers.Application.Commands.Common;

namespace PayNexa.Customers.Application.Commands.SuspendCustomer;

public sealed record SuspendCustomerCommand(Guid CustomerId, string Reason) : ICustomerCommand;
