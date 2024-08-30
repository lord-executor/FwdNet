using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace FwdNet;

/// <summary>
/// Parses a custom URI scheme that is derived from the PowerShell certificate provider which identifies installed
/// certificates with paths like "Cert:\CurrentUser\My\C7A099A497762360175556422EDC82861BC5D39E" with the certificate
/// thumbprint used as an identifier.
/// The scheme used here is extended to allow for
/// - case-insensitive matching of scheme, store location and store name
/// - either "/" or "\" used as the path separator
/// - matching certificates by subject by specifying "CN=&lt;cert-common-name&gt;" instead of a thumbprint
///   like e.g. "cert:/CurrentUser/My/CN=*.fwd.local"
/// </summary>
public partial class CertUriParser
{
    [GeneratedRegex(@"cert:/", RegexOptions.IgnoreCase)]
    private partial Regex CertSchemeRegex();
    
    [GeneratedRegex(@"cert:/([^/]+)/([^/]+)/(.*)", RegexOptions.IgnoreCase)]
    private partial Regex CertSegmentsRegex();
    
    public bool IsCertUrl(string uri) => CertSchemeRegex().IsMatch(Normalize(uri));

    public X509Certificate2Collection GetCertificates(string uri)
    {
        var match = CertSegmentsRegex().Match(Normalize(uri));
        if (!match.Success)
        {
            throw new ArgumentException($"Invalid certificate URI: {uri}", nameof(uri));
        }
        
        var storeLocation = (StoreLocation)Enum.Parse(typeof(StoreLocation), match.Groups[1].Value, ignoreCase: true);
        var storeName = (StoreName)Enum.Parse(typeof(StoreName), match.Groups[2].Value, ignoreCase: true);
        var certMatcher = match.Groups[3].Value;
        
        using var store = new X509Store(storeName, storeLocation);
        store.Open(OpenFlags.ReadOnly);

        return certMatcher.StartsWith("CN=")
            ? store.Certificates.Find(X509FindType.FindBySubjectName, certMatcher.Substring(3), false)
            : store.Certificates.Find(X509FindType.FindByThumbprint, certMatcher, false);
    }
    
    private static string Normalize(string uri) => uri.Replace('\\', '/');
}