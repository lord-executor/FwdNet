using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;

namespace FwdNet;

public class ForwardingProxyConfig : IProxyConfig
{
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();

    private readonly List<RouteConfig> _routes = new List<RouteConfig>();
    private readonly List<ClusterConfig> _clusters = new List<ClusterConfig>();

    public IReadOnlyList<RouteConfig> Routes => _routes;
    public IReadOnlyList<ClusterConfig> Clusters => _clusters;
    public IChangeToken ChangeToken { get; }

    public ForwardingProxyConfig(ForwardingConfig config)
    {
        ChangeToken = new CancellationChangeToken(_cts.Token);

        var index = 0;
        foreach (var rule in config.Rules)
        {
            var clusterId = $"target-{index}";
            _clusters.Add(new ClusterConfig
            {
                ClusterId = clusterId,
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    ["main"] = new DestinationConfig
                    {
                        Address = rule.Forward,
                    }
                }
            });

            var sourceUri = new Uri(rule.Listen);
            _routes.Add(new RouteConfig
            {
                RouteId = $"route-{index}",
                ClusterId = clusterId,
                Match = new RouteMatch
                {
                    Path = "{**rest}",
                    Hosts = [rule.ListenHost ?? sourceUri.Host]
                }
            });

            index++;
        }
    }
}