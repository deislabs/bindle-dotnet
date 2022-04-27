using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Deislabs.Bindle;

public class InvoiceWriter
{
    public static string Write(Invoice invoice)
    {
        return new InvoiceWriter().WriteInvoice(invoice);
    }

    private readonly StringBuilder _sb = new StringBuilder();

    private string WriteInvoice(Invoice invoice)
    {
        Write("bindleVersion", invoice.BindleVersion);

        if (invoice.Bindle is not null)
        {
            WriteTableStart("bindle");
            Write("name", invoice.Bindle.Name);
            Write("version", invoice.Bindle.Version);
            Write("description", invoice.Bindle.Description);
            Write("authors", invoice.Bindle.Authors);
        }

        if (invoice.Annotations.Any())
        {
            WriteTableStart("annotations");
            foreach (var kvp in invoice.Annotations)
            {
                Write(kvp.Key, kvp.Value);
            }
        }

        foreach (var parcel in invoice.Parcels)
        {
            WriteTableArrayStart("parcel");
            if (parcel.Label is not null)
            {
                WriteTableStart("parcel.label");
                Write("name", parcel.Label.Name);
                Write("sha256", parcel.Label.Sha256);
                Write("mediaType", parcel.Label.MediaType);
                Write("size", parcel.Label.Size);

                if (parcel.Label.Annotations.Any())
                {
                    WriteTableStart("parcel.label.annotations");
                    foreach (var kvp in parcel.Label.Annotations)
                    {
                        Write(kvp.Key, kvp.Value);
                    }
                }

                foreach (var feature in parcel.Label.Feature)
                {
                    if (feature.Value.Any())
                    {
                        WriteTableStart("parcel.label.feature." + feature.Key);
                        foreach (var kvp in feature.Value)
                        {
                            Write(kvp.Key, kvp.Value);
                        }
                    }
                }
            }
        }

        foreach (var group in invoice.Groups)
        {
            WriteTableArrayStart("group");
            Write("name", group.Name);
            Write("required", group.Required);
            Write("satisfiedBy", TomliseSatisfiedBy(group.SatisfiedBy));
        }

        return _sb.ToString();
    }

    private void Write(string key, string? value)
    {
        if (value is not null)
        {
            _sb.AppendLine($"{key} = \"{value}\"");
        }
    }

    private void Write(string key, long value)
    {
        _sb.AppendLine($"{key} = {value}");
    }

    private void Write(string key, bool value)
    {
        _sb.AppendLine($"{key} = {value.ToString().ToLowerInvariant()}");
    }

    private void Write(string key, IEnumerable<string> value)
    {
        var array = String.Join(',', value.Select(v => '"' + v + '"'));
        _sb.AppendLine($"{key} = [ {array} ]");
    }

    private void WriteTableStart(string key)
    {
        _sb.AppendLine($"[{key}]");
    }

    private void WriteTableArrayStart(string key)
    {
        _sb.AppendLine($"[[{key}]]");
    }

    internal static string TomliseSatisfiedBy(SatisfiedBy value)
    {
        return value switch
        {
            SatisfiedBy.AllOf => "allOf",
            SatisfiedBy.OneOf => "oneOf",
            SatisfiedBy.Optional => "optional",
            _ => throw new InvalidOperationException($"Unknown SatisfiedBy value {value}"),
        };
    }
}
