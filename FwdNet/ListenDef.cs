using System.Net;
using System.Text.RegularExpressions;

namespace FwdNet;

public partial record ListenDef(IPAddress Address, int Port)
{
    [GeneratedRegex(@"^\*:(\d{1,5})$")]
    private static partial Regex AnyIpRegex();

    public static bool IsAnyIp(string value)
    {
        return AnyIpRegex().IsMatch(value);
    }

    public static ListenDef FromAnyIp(string value)
    {
        var match = AnyIpRegex().Match(value);
        if (!match.Success)
        {
            throw new ArgumentException("Value is not a valid ANY IP expression", nameof(value));
        }

        return new ListenDef(IPAddress.Any, int.Parse(match.Groups[1].Value));
    }
    
    public static ListenDef FromUri(Uri uri)
    {
        return new ListenDef(IPAddress.Parse(uri.Host), uri.Port);
    }
}