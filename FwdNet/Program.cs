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

builder.WebHost.UseUrls(forwardingConfig.Rules.Select(r => r.Listen).ToArray());
builder.Services.AddReverseProxy();

var app = builder.Build();
app.MapReverseProxy();

app.Run();
return 0;
