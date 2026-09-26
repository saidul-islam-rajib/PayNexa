using PayNexa.Customers.Application.Commands.Common;

namespace PayNexa.Customers.Application.Commands.CloseCustomer;

public sealed record CloseCustomerCommand(Guid CustomerId, string Reason) : ICustomerCommand;
