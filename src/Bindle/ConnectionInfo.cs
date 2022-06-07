using System;
using System.Collections.Generic;
using System.Linq;

namespace Deislabs.Bindle;

public class ConnectionInfo
{
    private readonly Dictionary<string, string> keyValuePairs;

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

    public string BaseUri;

    public SslMode? SslMode;

    public ConnectionInfo(string connectionString)
    {
        keyValuePairs = GetKeyValuePairs(connectionString);

        BaseUri = GetValue(serverAliases);

        try
        {
            SslMode = Enum.Parse<SslMode>(GetValue(sslModeAliases), true);
        }
        catch (Exception)
        {
            SslMode = null;
        }
    }

    private Dictionary<string, string> GetKeyValuePairs(string connectionString)
    {
        return connectionString.Split(';')
            .Where(kvp => kvp.Contains('='))
            .Select(kvp => kvp.Split(new char[] { '=' }, 2))
            .ToDictionary(kvp => kvp[0].Trim(),
                          kvp => kvp[1].Trim(),
                          StringComparer.InvariantCultureIgnoreCase);
    }

    private string GetValue(string[] keyAliases)
    {
        foreach (var alias in keyAliases)
        {
            string? value;
            if (keyValuePairs.TryGetValue(alias, out value))
                return value;
        }
        return string.Empty;
    }
}
