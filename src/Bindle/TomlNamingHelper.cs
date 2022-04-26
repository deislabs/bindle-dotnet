using System;
using System.Text;

namespace Deislabs.Bindle;

public class TomlNamingHelper
{
    [ThreadStatic]
    private static StringBuilder? _builder;

    public static string PascalToCamelCase(string name)
    {
        var builder = Builder;
        try
        {
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (i == 0)
                {
                    c = char.ToLowerInvariant(c);
                }
                builder.Append(c);
            }
            return builder.ToString();
        }
        finally
        {
            builder.Length = 0;
        }
    }

    private static StringBuilder Builder => _builder ??= new StringBuilder();
}
