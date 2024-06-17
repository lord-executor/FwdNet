namespace FwdNet;

public class ForwardingRule
{
    public string Listen { get; set; } = string.Empty;
    public string? ListenHost { get; set; }
    public string Forward { get; set; } = string.Empty;
    public string? Certificate { get; set; } = string.Empty;
    public string? CertificatePassword { get; set; } = string.Empty;
}