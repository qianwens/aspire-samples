using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDiscovery.Models
{
    public class ServiceInstance
    {
        public string? Id { get; set; }
        public string? Description { get; set; }
        public string? Address { get; set; }
        public int Port { get; set; }
        public string HealthStatus { get; } = "healthy";
        public IDictionary<string, string>? Metadatas { get; set; }

    }
}
