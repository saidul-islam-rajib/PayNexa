using PayNexa.Customers.Application.Commands.Common;

namespace PayNexa.Customers.Application.Commands.ChangeCustomerEmail;

public sealed record ChangeCustomerEmailCommand(Guid CustomerId, string Email) : ICustomerCommand;
