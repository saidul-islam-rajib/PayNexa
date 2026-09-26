using System.ComponentModel.DataAnnotations;

namespace PayNexa.Caching;

public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    [Required]
    public string ConnectionString { get; set; } = string.Empty;
}
