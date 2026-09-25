using PayNexa.Customers.Application.Commands.Common;

namespace PayNexa.Customers.Application.Commands.UpdateCustomerProfile;

public sealed record UpdateCustomerProfileCommand(Guid CustomerId, string FirstName, string LastName, string PhoneNumber) : ICustomerCommand;
