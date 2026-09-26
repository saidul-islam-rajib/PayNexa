using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using PayNexa.Common.Configuration;

namespace PayNexa.Logging;

public sealed record ServiceIdentity(string Name, string Version, string Environment)
{
    public const string NameConfigurationKey = ServiceConfigurationKeys.ServiceName;
    public const string MissingNameMessage = "Configuration value 'Service:Name' is required so every log event and trace carries its service name.";

    public static ServiceIdentity From(IConfiguration configuration, IHostEnvironment environment)
    {
        var name = configuration[NameConfigurationKey];

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException(MissingNameMessage);
        }

        var version = Assembly.GetEntryAssembly()?
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "0.0.0";

        return new ServiceIdentity(name, version, environment.EnvironmentName);
    }
}
