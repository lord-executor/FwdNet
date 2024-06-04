var builder = WebApplication.CreateBuilder(args);

// Notes:
// - https://learn.microsoft.com/en-us/dotnet/core/additional-tools/self-signed-certificates-guide (With PowerShell)
// - https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-8.0#configure-https-in-code

builder.WebHost.UseUrls("https://127.0.42.1");
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();
app.MapReverseProxy();

app.Run();
