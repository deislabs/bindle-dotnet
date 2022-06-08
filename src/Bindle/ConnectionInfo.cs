using System;
using System.Collections.Generic;
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

    static readonly string[] usernameAliases = {
        "user",
        "username"
    };

    static readonly string[] passwordAliases = {
        "pass",
        "passwd",
        "password"
    };

    private readonly Dictionary<string, string> keyValuePairs;

    public string BaseUri;

    public string UserName;

    public string Password;

    public SslMode? SslMode;

    public ConnectionInfo()
    {
        keyValuePairs = new Dictionary<string, string>();
        BaseUri = "http://localhost:8080/v1/";
        UserName = String.Empty;
        Password = String.Empty;
    }

    public ConnectionInfo(string connectionString)
    {
        keyValuePairs = GetKeyValuePairs(connectionString);

        BaseUri = GetValue(serverAliases);

        UserName = GetValue(usernameAliases);

        Password = GetValue(passwordAliases);

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
