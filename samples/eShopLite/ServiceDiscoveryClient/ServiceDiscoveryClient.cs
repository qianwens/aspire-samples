using ServiceDiscovery.Models;
using System.Net.Http;
using System.Web;
using System.Threading;
using System.Net.Http.Json;

namespace ServiceDiscovery
{
    public class ServiceDiscoveryClient
    {
        private HttpClient _client;
        public ServiceDiscoveryClient(string serviceDiscoveryUrl)
        {
            _client = new()
            {
                BaseAddress = new Uri(serviceDiscoveryUrl),
            };
        }

        public async Task<IEnumerable<ServiceInstance>?> GetServiceInstances(string namespaceName, string serviceName, CancellationToken cancellationToken = default)
        {
            // Call the service discovery endpoint to get the list of service instances
            var response = await _client.GetAsync($"namespaces/{namespaceName}/services/{serviceName}/instances", cancellationToken).ConfigureAwait(false);
            return await response.Content.ReadFromJsonAsync<IEnumerable<ServiceInstance>>();
        }

        public async Task<IEnumerable<Service>?> GetServices(string namespaceName, CancellationToken cancellationToken = default)
        {
            // Call the service discovery endpoint to get the list of service instances
            var response = await _client.GetAsync($"namespaces/{namespaceName}/services", cancellationToken).ConfigureAwait(false);
            return await response.Content.ReadFromJsonAsync<IEnumerable<Service>>();
        }
    }
}