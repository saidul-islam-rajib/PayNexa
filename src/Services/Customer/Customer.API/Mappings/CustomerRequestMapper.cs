using PayNexa.Customers.API.Requests;
using PayNexa.Customers.Application.Commands.ChangeCustomerAddress;
using PayNexa.Customers.Application.Commands.ChangeCustomerEmail;
using PayNexa.Customers.Application.Commands.CloseCustomer;
using PayNexa.Customers.Application.Commands.RegisterCustomer;
using PayNexa.Customers.Application.Commands.RejectCustomerKyc;
using PayNexa.Customers.Application.Commands.SuspendCustomer;
using PayNexa.Customers.Application.Commands.UpdateCustomerProfile;
using PayNexa.Customers.Application.Queries.ListCustomers;
using PayNexa.Customers.Contracts.Requests;
using Riok.Mapperly.Abstractions;

namespace PayNexa.Customers.API.Mappings;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal static partial class CustomerRequestMapper
{
    public static partial RegisterCustomerCommand ToCommand(this RegisterCustomerRequest request);

    public static partial UpdateCustomerProfileCommand ToCommand(this UpdateCustomerProfileRequest request, Guid customerId);

    public static partial ChangeCustomerEmailCommand ToCommand(this ChangeCustomerEmailRequest request, Guid customerId);

    public static partial ChangeCustomerAddressCommand ToCommand(this ChangeCustomerAddressRequest request, Guid customerId);

    public static partial SuspendCustomerCommand ToSuspendCommand(this CustomerStatusChangeRequest request, Guid customerId);

    public static partial CloseCustomerCommand ToCloseCommand(this CustomerStatusChangeRequest request, Guid customerId);

    public static partial RejectCustomerKycCommand ToRejectKycCommand(this CustomerStatusChangeRequest request, Guid customerId);

    public static ListCustomersQuery ToQuery(this ListCustomersRequest request) =>
        new(request.ToPageRequest(), request.Search, request.Status, request.KycStatus, request.SortBy, request.SortOrder);
}
