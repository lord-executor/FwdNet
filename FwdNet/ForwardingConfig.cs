using System.Text.Json;

namespace FwdNet;

public class ForwardingConfig
{
    public IList<ForwardingRule> Rules { get; set; }

    public ForwardingConfig(string configFile)
    {
        Rules = JsonSerializer.Deserialize<IList<ForwardingRule>>(File.ReadAllText(configFile))
            ?? throw new ArgumentException($"Error parsing {configFile}");
    }
}