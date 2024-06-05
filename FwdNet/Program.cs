using System.Net;
using FwdNet;
using Yarp.ReverseProxy.Configuration;

if (args.Length < 1)
{
    Console.WriteLine("Must provide forwarding configuration file name");
    return -1;
}

var forwardingConfigFile = args[0];
if (!File.Exists(forwardingConfigFile))
{
    Console.WriteLine("Forwarding configuration file does not exist");
    return -2;
}

var forwardingConfig = new ForwardingConfig(forwardingConfigFile);

var builder = WebApplication.CreateBuilder(args);

// Notes:
// - https://learn.microsoft.com/en-us/dotnet/core/additional-tools/self-signed-certificates-guide (With PowerShell)
// - https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-8.0#configure-https-in-code

builder.Services.AddSingleton(forwardingConfig);
builder.Services.AddSingleton<IProxyConfigProvider, ForwardingProxyConfigProvider>();

builder.WebHost.ConfigureKestrel((context, options) =>
{
    foreach (var rule in forwardingConfig.Rules)
    {
        var listenUri = new Uri(rule.Listen);
        options.Listen(IPAddress.Parse(listenUri.Host), listenUri.Port,
            listenOptions => { listenOptions.UseHttps("certificates/fwd.local.pfx", "fwd"); });
    }
});

builder.Services.AddReverseProxy();

var app = builder.Build();
app.MapReverseProxy();

app.Run();
return 0;
