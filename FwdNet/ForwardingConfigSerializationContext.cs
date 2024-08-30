using System.Text.Json.Serialization;

namespace FwdNet;

[JsonSourceGenerationOptions(
    WriteIndented = true/*,
    GenerationMode = JsonSourceGenerationMode.Serialization*/)]
[JsonSerializable(typeof(IList<ForwardingRule>))]
[JsonSerializable(typeof(ForwardingRule))]
public partial class ForwardingConfigSerializationContext : JsonSerializerContext
{
}