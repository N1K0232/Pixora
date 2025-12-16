using Microsoft.AspNetCore.DataProtection;
using Pixora.DataProtectionLayer;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IDataProtectionBuilder AddDataProtection(this IServiceCollection services, string applicationName)
    {
        var builder = services.AddDataProtection().SetApplicationName(applicationName);

        services.AddSingleton(provider =>
        {
            var dataProtectionProvider = provider.GetRequiredService<IDataProtectionProvider>();
            var dataProtector = dataProtectionProvider.CreateProtector(applicationName);

            return dataProtector;
        });

        services.AddSingleton(provider =>
        {
            var dataProtector = provider.GetRequiredService<IDataProtector>();
            return dataProtector.ToTimeLimitedDataProtector();
        });

        services.AddSingleton<IDataProtectionService, DataProtectionService>();
        services.AddSingleton<ITimeLimitedDataProtectionService, TimeLimitedDataProtectionService>();

        return builder;
    }
}