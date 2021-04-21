using System;
using System.Collections.Generic;
using System.Linq;
using Tomlyn.Model;

namespace Bindle
{
    internal static class Parser
    {

        internal static Invoice ParseInvoice(TomlTable toml)
        {
            var bindleVersion = toml.GetString("bindleVersion");
            if (bindleVersion != "1.0.0")
            {
                throw new System.Exception($"Unknown Bindle version {bindleVersion}");
            }

            var yanked = toml.TryGetBool("yanked") ?? false;

            var bindle = ParseBindleMetadata(toml.GetTomlTable("bindle"));
            var annotations = ParseStringDictionary(toml.TryGetTomlTable("annotations"));
            var parcels = ParseParcels(toml.TryGetTomlTables("parcel"));
            var groups = ParseGroups(toml.TryGetTomlTables("group"));

            return new Invoice(
                bindleVersion,
                yanked,
                bindle,
                annotations,
                parcels,
                groups
            );
        }

        internal static BindleMetadata ParseBindleMetadata(TomlTable toml)
        {
            var name = toml.GetString("name");
            var version = toml.GetString("version");
            var description = toml.TryGetString("description");
            var authors = toml.TryGetStringArray("authors") ?? new string[0];
            return new BindleMetadata(name, version, description, authors);
        }

        internal static IDictionary<string, string> ParseStringDictionary(TomlTable? toml)
        {
            if (toml is null)
            {
                return new Dictionary<string, string>();
            }

            var kvps = toml.Keys.Select(k => KeyValuePair.Create(k, toml.GetString(k)));
            return new Dictionary<string, string>(kvps);
        }

        internal static IEnumerable<Parcel> ParseParcels(IEnumerable<TomlTable>? toml)
        {
            if (toml is null)
            {
                return new List<Parcel>();
            }

            return toml.Select(ParseParcel);
        }

        internal static Parcel ParseParcel(TomlTable toml)
        {
            var label = ParseLabel(toml.GetTomlTable("label"));
            var conditions = ParseConditions(toml.TryGetTomlTable("conditions"));
            return new Parcel(label, conditions);
        }

        internal static Label ParseLabel(TomlTable toml)
        {
            var name = toml.GetString("name");
            var sha256 = toml.GetString("sha256");
            var mediaType = toml.GetString("mediaType");
            var size = toml.GetLong("size");
            var annotations = ParseStringDictionary(toml.TryGetTomlTable("annotations"));
            var feature = ParseFeatureDictionary(toml.TryGetTomlTable("feature"));

            return new Label(
                name,
                sha256,
                mediaType,
                size,
                annotations,
                feature
            );
        }

        internal static IDictionary<string, IDictionary<string, string>> ParseFeatureDictionary(TomlTable? toml)
        {
            if (toml is null)
            {
                return new Dictionary<string, IDictionary<string, string>>();
            }

            var kvps = toml.Keys.Select(k => KeyValuePair.Create(k, ParseStringDictionary(toml.GetTomlTable(k))));
            return new Dictionary<string, IDictionary<string, string>>(kvps);
        }

        internal static Conditions? ParseConditions(TomlTable? toml)
        {
            if (toml is null)
            {
                return null;
            }

            var memberOf = toml.TryGetStringArray("memberOf") ?? new string[0];
            var requires = toml.TryGetStringArray("requires") ?? new string[0];

            return new Conditions(memberOf, requires);
        }

        internal static IEnumerable<Group> ParseGroups(IEnumerable<TomlTable>? toml)
        {
            if (toml is null)
            {
                return new List<Group>();
            }

            return toml.Select(ParseGroup);
        }

        internal static Group ParseGroup(TomlTable toml)
        {
            var name = toml.GetString("name");
            var required = toml.TryGetBool("required") ?? false;
            var satisfiedBy = Enum.Parse<SatisfiedBy>(toml.TryGetString("satisfiedBy") ?? "allOf", true);
            return new Group(name, required, satisfiedBy);
        }
    }

    internal static class TomlHelpers
    {
        internal static TomlObject? TryGet(this TomlTable toml, string key)
        {
            if (toml.TryGetToml(key, out var t))
            {
                return t;
            }
            return null;
        }

        internal static string? TryGetString(this TomlTable toml, string key)
        {
            if (toml.TryGetToml(key, out var t))
            {
                if (t.Kind == ObjectKind.String)
                {
                    return ((TomlString)t).Value;
                }
                else
                {
                    throw new System.Exception($"Invalid field {key}: expected string but got ${t.Kind.ToString()}");
                }
            }
            return null;
        }

        internal static bool? TryGetBool(this TomlTable toml, string key)
        {
            if (toml.TryGetToml(key, out var t))
            {
                if (t.Kind == ObjectKind.Boolean)
                {
                    return ((TomlBoolean)t).Value;
                }
                else
                {
                    throw new System.Exception($"Invalid field {key}: expected boolean but got ${t.Kind.ToString()}");
                }
            }
            return null;
        }

        internal static IEnumerable<string>? TryGetStringArray(this TomlTable toml, string key)
        {
            if (toml.TryGetToml(key, out var t))
            {
                if (t.Kind == ObjectKind.Array)
                {
                    var arr = (TomlArray)t;
                    return arr.Cast<string>();
                }
                else
                {
                    throw new System.Exception($"Invalid field {key}: expected string array but got ${t.Kind.ToString()}");
                }
            }
            return null;
        }

        internal static TomlTable? TryGetTomlTable(this TomlTable toml, string key)
        {
            if (toml.TryGetToml(key, out var t))
            {
                if (t.Kind == ObjectKind.Table)
                {
                    return (TomlTable)t;
                }
                else
                {
                    throw new System.Exception($"Invalid field {key}: expected section but got ${t.Kind.ToString()}");
                }
            }
            return null;
        }

        internal static IEnumerable<TomlTable>? TryGetTomlTables(this TomlTable toml, string key)
        {
            if (toml.TryGetToml(key, out var t))
            {
                if (t.Kind == ObjectKind.TableArray)
                {
                    return (TomlTableArray)t;
                }
                else
                {
                    throw new System.Exception($"Invalid field {key}: expected sections but got ${t.Kind.ToString()}");
                }
            }
            return null;
        }

        internal static TomlObject Get(this TomlTable toml, string key)
        {
            if (toml.TryGetToml(key, out var t))
            {
                return t;
            }
            throw new System.Exception($"Missing field {key}");
        }

        internal static string GetString(this TomlTable toml, string key)
        {
            if (toml.TryGetToml(key, out var t))
            {
                if (t.Kind == ObjectKind.String)
                {
                    return ((TomlString)t).Value;
                }
                else
                {
                    throw new System.Exception($"Invalid field {key}: expected string but got ${t.Kind.ToString()}");
                }
            }
            throw new System.Exception($"Missing field {key}");
        }

        internal static long GetLong(this TomlTable toml, string key)
        {
            if (toml.TryGetToml(key, out var t))
            {
                if (t.Kind == ObjectKind.Integer)
                {
                    return ((TomlInteger)t).Value;
                }
                else
                {
                    throw new System.Exception($"Invalid field {key}: expected integer but got ${t.Kind.ToString()}");
                }
            }
            throw new System.Exception($"Missing field {key}");
        }

        internal static TomlTable GetTomlTable(this TomlTable toml, string key)
        {
            if (toml.TryGetToml(key, out var t))
            {
                if (t.Kind == ObjectKind.Table)
                {
                    return (TomlTable)t;
                }
                else
                {
                    throw new System.Exception($"Invalid field {key}: expected section but got ${t.Kind.ToString()}");
                }
            }
            throw new System.Exception($"Missing section {key}");
        }
    }
}
