using System.ComponentModel.DataAnnotations;
using PayNexa.Customers.Domain.CustomerAggregate.Policies;

namespace PayNexa.Customers.Application.Options;

public sealed class CustomerOptions
{
    public const string SectionName = "Customer";

    [Range(0, 100)]
    public int MinimumAgeYears { get; set; } = CustomerPolicy.DefaultMinimumAgeYears;

    [Range(typeof(TimeSpan), "00:00:10", "1.00:00:00")]
    public TimeSpan ProfileCacheTimeToLive { get; set; } = TimeSpan.FromMinutes(10);

    public CustomerPolicy ToPolicy() => new(MinimumAgeYears);
}
