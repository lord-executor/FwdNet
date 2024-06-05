using Yarp.ReverseProxy.Configuration;

namespace FwdNet;

public class ForwardingProxyConfigProvider : IProxyConfigProvider
{
    private readonly ForwardingConfig _config;

    public ForwardingProxyConfigProvider(ForwardingConfig config)
    {
        _config = config;
    }
    
    public IProxyConfig GetConfig()
    {
        return new ForwardingProxyConfig(_config);
    }
}