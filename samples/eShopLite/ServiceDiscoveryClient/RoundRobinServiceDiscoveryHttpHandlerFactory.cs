using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

namespace ServiceDiscovery
{
    public class RoundRobinServiceDiscoveryHttpHandlerFactory
    {
        private ServiceDiscoveryClient _client;
        private string _namespaceName;
        
        public RoundRobinServiceDiscoveryHttpHandlerFactory(ServiceDiscoveryClient client, string namespaceName)
        {
            _client = client;
            _namespaceName = namespaceName;
        }
        
        public SocketsHttpHandler Create()
        {
            var socketHttpHandler = new SocketsHttpHandler()
            {
                PooledConnectionIdleTimeout = TimeSpan.FromSeconds(1),
                PooledConnectionLifetime = TimeSpan.FromSeconds(1),

                ConnectCallback = async (context, cancellationToken) =>
                {
                    IPAddress[] addresses;
                    var instances = await _client.GetServiceInstances(_namespaceName, context.DnsEndPoint.Host, cancellationToken).ConfigureAwait(false);
                    // if there is no instances, fallback to DNS resolution
                    if (instances?.Any() == false)
                    {
                        var entry = await Dns.GetHostEntryAsync(context.DnsEndPoint.Host, cancellationToken);
                        if (entry.AddressList.Length == 1)
                            addresses = entry.AddressList;
                        else
                            addresses = IpAddressRouter.Update(entry);
                    }
                    else {
                        if (instances?.Count() == 1)
                            addresses = await ServiceInstanceAddress.Parse(instances.First().Address!, cancellationToken).ConfigureAwait(false);
                        else
                        {
                            List<IPAddress> discoveredAddresses = new List<IPAddress>();
                            var healthyInstances = instances!.Where((i) => i.HealthStatus == "healthy");
                            foreach (var instance in healthyInstances)
                            {
                                var instanceAddressed = await ServiceInstanceAddress.Parse(instance.Address!, cancellationToken).ConfigureAwait(false);
                                discoveredAddresses = discoveredAddresses.Concat(instanceAddressed).ToList();
                            }
                            addresses = IpAddressRouter.Update(context.DnsEndPoint.Host, discoveredAddresses.ToArray());
                        }
                    }
                    var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                    try
                    {
                        await socket.ConnectAsync(addresses, context.DnsEndPoint.Port, cancellationToken);
                        return new NetworkStream(socket, ownsSocket: true);
                    }
                    catch
                    {
                        socket.Dispose();
                        throw;
                    }
                }
            };

            return socketHttpHandler;
        }
    }

    internal static class ServiceInstanceAddress
    {
        internal static async Task<IPAddress[]> Parse(string address, CancellationToken cancellationToken)
        {
            IPAddress? ip;
            if (IPAddress.TryParse(address, out ip))
            {
                return new[] { ip };
            }
            else
            {
                var entry = await Dns.GetHostEntryAsync(address, cancellationToken).ConfigureAwait(false);
                return entry.AddressList;
            }
        }
    }

    internal static class IpAddressRouter
    {
        private static readonly ConcurrentDictionary<string, int> ipAddresses = new(StringComparer.OrdinalIgnoreCase);

        public static IPAddress[] Update(IPHostEntry entry)
        {
            return Update(entry.HostName, entry.AddressList);
        }

        public static IPAddress[] Update(string hostName, IPAddress[] addressList)
        {
            var index = ipAddresses.AddOrUpdate(hostName, 0, (_, existingValue) => existingValue + 1);

            index %= addressList.Length;

            IPAddress[] addresses;
            if (index == 0)
                addresses = addressList;
            else
            {
                addresses = new IPAddress[addressList.Length];
                addressList.AsSpan(index).CopyTo(addresses);
                addressList.AsSpan(0, index).CopyTo(addresses.AsSpan(index));
            }

            return addresses;
        }
    }
}
