using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace PayNexa.Logging;

public sealed record ServiceIdentity(string Name, string Version, string Environment)
{
    public const string NameConfigurationKey = "Service:Name";

    public static ServiceIdentity From(IHostApplicationBuilder builder)
    {
        var name = builder.Configuration[NameConfigurationKey];

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException(
                $"Configuration value '{NameConfigurationKey}' is required so every log event and trace carries its service name.");
        }

        var version = Assembly.GetEntryAssembly()?
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "0.0.0";

        return new ServiceIdentity(name, version, builder.Environment.EnvironmentName);
    }
}
