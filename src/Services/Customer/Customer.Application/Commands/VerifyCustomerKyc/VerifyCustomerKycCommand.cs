using PayNexa.Customers.Application.Commands.Common;

namespace PayNexa.Customers.Application.Commands.VerifyCustomerKyc;

public sealed record VerifyCustomerKycCommand(Guid CustomerId) : ICustomerCommand;
