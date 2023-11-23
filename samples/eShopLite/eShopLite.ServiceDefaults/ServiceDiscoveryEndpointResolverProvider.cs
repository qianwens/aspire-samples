using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ServiceDiscovery;

namespace Microsoft.Extensions.ServiceDiscovery.Abstractions;

//
// Summary:
//     Microsoft.Extensions.ServiceDiscovery.Abstractions.IServiceEndPointResolverProvider
//     implementation that resolves services using Microsoft.Extensions.Configuration.IConfiguration.
//
//
// Parameters:
//   configuration:
//     The configuration.
//
//   options:
//     The options.
public class ServiceDiscoveryEndPointResolverProvider : IServiceEndPointResolverProvider
{
    //
    // Summary:
    //     Microsoft.Extensions.ServiceDiscovery.Abstractions.IServiceEndPointResolverProvider
    //     implementation that resolves services using Microsoft.Extensions.Configuration.IConfiguration.
    //
    //
    // Parameters:
    //   configuration:
    //     The configuration.
    //
    //   options:
    //     The options.
    ServiceDiscoveryClient? _client;
    string? _namespaceName;
    public ServiceDiscoveryEndPointResolverProvider()
    {
        var url = Environment.GetEnvironmentVariable("ServiceDiscovery");
        var nameSpace = Environment.GetEnvironmentVariable("Namespace");
        if (url == null || nameSpace == null)
        {
            return;
        }
        _client = new(url);
        _namespaceName = nameSpace;   
    }

    public bool TryCreateResolver(string serviceName, [NotNullWhen(true)] out IServiceEndPointResolver? resolver)
    {
        if (_client == null || _namespaceName == null)
        {
            resolver = null;
            return false;
        }
        resolver = new ServiceDiscoveryEndPointResolver(serviceName, _client, _namespaceName);
        return true;
    }
}
