using System.Text.Json;

namespace FwdNet;

public class ForwardingConfig
{
    public IList<ForwardingRule> Rules { get; set; }

    public ForwardingConfig(string configFile)
    {
        Rules = JsonSerializer.Deserialize(File.ReadAllText(configFile), ForwardingConfigSerializationContext.Default.IListForwardingRule)
            ?? throw new ArgumentException($"Error parsing {configFile}");
    }
}