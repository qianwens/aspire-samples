using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.ServiceDiscovery.Internal;
using Polly;
using ServiceDiscovery;

namespace Microsoft.Extensions.ServiceDiscovery.Abstractions;

//
// Summary:
//     A service endpoint resolver that uses configuration to resolve endpoints.
internal sealed class ServiceDiscoveryEndPointResolver : IServiceEndPointResolver, IAsyncDisposable
{
    private readonly string _serviceName;

    private readonly string? _endpointName;

    private readonly string _namespaceName;

    private ServiceDiscoveryClient _client;

    private readonly string _displayName = "servicediscovery";

    //
    // Summary:
    //     Initializes a new Microsoft.Extensions.ServiceDiscovery.Abstractions.ConfigurationServiceEndPointResolver
    //     instance.
    //
    // Parameters:
    //   serviceName:
    //     The service name.
    //
    //   configuration:
    //     The configuration.
    //
    //   options:
    //     The options.
    public ServiceDiscoveryEndPointResolver(string serviceName, ServiceDiscoveryClient client, string namespaceName)
    {
        if (ServiceNameParts2.TryParse(serviceName, out var parts))
        {
            _serviceName = parts.Host;
            _endpointName = parts.EndPointName;
            _client = client;
            _namespaceName = namespaceName;
            return;
        }

        throw new InvalidOperationException("Service name '" + serviceName + "' is not valid");
    }

    public string DisplayName { get { return _displayName; } }
    public ValueTask DisposeAsync()
    {
        return default(ValueTask);
    }

    public ValueTask<ResolutionStatus> ResolveAsync(ServiceEndPointCollectionSource endPoints, CancellationToken cancellationToken)
    {
        return new ValueTask<ResolutionStatus>(ResolveInternal(endPoints));
    }

    private ResolutionStatus ResolveInternal(ServiceEndPointCollectionSource endPoints)
    {
        if (endPoints.EndPoints.Count != 0)
        {
            return ResolutionStatus.None;
        }

        var instances =  _client.GetServiceInstances(_namespaceName, _serviceName).Result;
        if (instances == null || instances.Count() == 0)
        {
            return ResolutionStatus.CreateNotFound($"No service instance for " + _serviceName + "   was found");
        }
        foreach (var item in instances)
        {
            if (ServiceNameParts2.TryParse(item.Address!, item.Port, out var parts))
            {
                if (SchemesMatch(_endpointName, parts))
                {
                    if (!ServiceNameParts2.TryCreateEndPoint(parts, out EndPoint endPoint))
                    {
                        return ResolutionStatus.FromException(new KeyNotFoundException("The address configured in service discovery for " + _serviceName + " is invalid."));
                    }

                    endPoints.EndPoints.Add(ServiceEndPoint.Create(endPoint));
                }
            }
        }

        return ResolutionStatus.Success;
        static bool SchemesMatch(string? scheme, ServiceNameParts2 parts)
        {
            if (!string.IsNullOrEmpty(parts.EndPointName) && !string.IsNullOrEmpty(scheme))
            {
                return MemoryExtensions.Equals(parts.EndPointName, scheme, StringComparison.OrdinalIgnoreCase);
            }

            return true;
        }
    }
    private static List<ServiceNameParts2> ParseServiceNameParts(List<string> input)
    {
        List<ServiceNameParts2> list = new List<ServiceNameParts2>(input.Count);
        for (int i = 0; i < input.Count; i++)
        {
            if (ServiceNameParts2.TryParse(input[i], out var parts))
            {
                list.Add(parts);
            }
        }

        return list;
    }
}
