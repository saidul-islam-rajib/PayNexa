namespace PayNexa.Customers.Domain.CustomerAggregate.Policies;

public sealed record CustomerPolicy(int MinimumAgeYears)
{
    public const int DefaultMinimumAgeYears = 18;

    public static readonly CustomerPolicy Default = new(DefaultMinimumAgeYears);

    public bool MeetsMinimumAge(DateOnly dateOfBirth, DateOnly today) =>
        dateOfBirth.AddYears(MinimumAgeYears) <= today;
}
