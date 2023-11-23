using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Microsoft.Extensions.ServiceDiscovery.Internal;

internal readonly struct ServiceNameParts2
{
    public string? EndPointName { get; init; }

    public string Host { get; init; }

    public int Port { get; init; }

    public ServiceNameParts2(string host, string? endPointName, int port)
    {
        this = default(ServiceNameParts2);
        Host = host;
        EndPointName = endPointName;
        Port = port;
    }

    public static bool TryParse(string serviceName, [NotNullWhen(true)] out ServiceNameParts2 parts)
    {
        if (serviceName.IndexOf("://") < 0 && Uri.TryCreate("fakescheme://" + serviceName, UriKind.RelativeOrAbsolute, out Uri result))
        {
            parts = Create(result, hasScheme: false);
            return true;
        }

        if (Uri.TryCreate(serviceName, UriKind.RelativeOrAbsolute, out result))
        {
            parts = Create(result, hasScheme: true);
            return true;
        }

        parts = default(ServiceNameParts2);
        return false;
        static ServiceNameParts2 Create(Uri uri, bool hasScheme)
        {
            string host = uri.Host;
            int num = host.IndexOf('.');
            string endPointName = null;
            string host2;
            if (host.StartsWith('_') && num > 1)
            {
                if (host[host.Length - 1] != '.')
                {
                    endPointName = host.Substring(1, num - 1);
                    string text = host;
                    int num2 = num + 1;
                    host2 = text.Substring(num2, text.Length - num2);
                    goto IL_0067;
                }
            }

            host2 = host;
            if (hasScheme)
            {
                endPointName = uri.Scheme;
            }

            goto IL_0067;
        IL_0067:
            return new ServiceNameParts2(host2, endPointName, uri.Port);
        }
    }

    public static bool TryParse(string serviceName, int port, [NotNullWhen(true)] out ServiceNameParts2 parts)
    {
        if (serviceName.IndexOf("://") < 0 && Uri.TryCreate("fakescheme://" + serviceName, UriKind.RelativeOrAbsolute, out Uri result))
        {
            parts = Create(result, port, hasScheme: false);
            return true;
        }

        if (Uri.TryCreate(serviceName, UriKind.RelativeOrAbsolute, out result))
        {
            parts = Create(result, port, hasScheme: true);
            return true;
        }

        parts = default(ServiceNameParts2);
        return false;
        static ServiceNameParts2 Create(Uri uri, int port, bool hasScheme)
        {
            string host = uri.Host;
            int num = host.IndexOf('.');
            string endPointName = null;
            string host2;
            if (host.StartsWith('_') && num > 1)
            {
                if (host[host.Length - 1] != '.')
                {
                    endPointName = host.Substring(1, num - 1);
                    string text = host;
                    int num2 = num + 1;
                    host2 = text.Substring(num2, text.Length - num2);
                    goto IL_0067;
                }
            }

            host2 = host;
            if (hasScheme)
            {
                endPointName = uri.Scheme;
            }

            goto IL_0067;
        IL_0067:
            return new ServiceNameParts2(host2, endPointName, port);
        }
    }

    public static bool TryCreateEndPoint(ServiceNameParts2 parts, [NotNullWhen(true)] out EndPoint? endPoint)
    {
        if (IPAddress.TryParse(parts.Host, out IPAddress address))
        {
            endPoint = new IPEndPoint(address, parts.Port);
        }
        else
        {
            if (string.IsNullOrEmpty(parts.Host))
            {
                endPoint = null;
                return false;
            }

            endPoint = new DnsEndPoint(parts.Host, 0);
        }

        return true;
    }


    public static bool TryCreateEndPoint(string serviceName, [NotNullWhen(true)] out EndPoint? serviceEndPoint)
    {
        if (TryParse(serviceName, out var parts))
        {
            return TryCreateEndPoint(parts, out serviceEndPoint);
        }

        serviceEndPoint = null;
        return false;
    }
}