using System;
using System.Linq;

namespace Deislabs.Bindle;

public class ConnectionInfo
{
    static readonly string[] serverAliases = {
        "server",
        "host",
        "data source",
        "datasource",
        "address",
        "addr",
        "network address",
    };

    static readonly string[] sslModeAliases = {
        "sslmode",
        "ssl mode"
    };

    public string? BaseUri;

    public SslMode? SslMode;

    public ConnectionInfo(string connectionString)
    {
        BaseUri = GetValue(connectionString, serverAliases);

        try
        {
            SslMode = Enum.Parse<SslMode>(GetValue(connectionString, sslModeAliases));
        }
        catch (Exception)
        {
            SslMode = null;
        }
    }

    static string GetValue(string connectionString, params string[] keyAliases)
    {
        var keyValuePairs = connectionString.Split(';')
                                            .Where(kvp => kvp.Contains('='))
                                            .Select(kvp => kvp.Split(new char[] { '=' }, 2))
                                            .ToDictionary(kvp => kvp[0].Trim(),
                                                        kvp => kvp[1].Trim(),
                                                        StringComparer.InvariantCultureIgnoreCase);
        foreach (var alias in keyAliases)
        {
            string? value;
            if (keyValuePairs.TryGetValue(alias, out value))
                return value;
        }
        return string.Empty;
    }
}
