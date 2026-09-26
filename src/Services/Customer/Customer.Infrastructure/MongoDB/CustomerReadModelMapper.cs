using PayNexa.Customers.Contracts.Events;
using PayNexa.Customers.Contracts.Responses;
using Riok.Mapperly.Abstractions;

namespace PayNexa.Customers.Infrastructure.MongoDB;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal static partial class CustomerReadModelMapper
{
    public static partial CustomerReadModel ToReadModel(this CustomerSnapshot snapshot);

    [MapperIgnoreSource(nameof(CustomerReadModel.Version))]
    public static partial CustomerResponse ToResponse(this CustomerReadModel readModel);
}
