using PayNexa.Customers.Application.Commands.Common;

namespace PayNexa.Customers.Application.Commands.RejectCustomerKyc;

public sealed record RejectCustomerKycCommand(Guid CustomerId, string Reason) : ICustomerCommand;
