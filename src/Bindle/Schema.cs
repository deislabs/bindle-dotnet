using System.Collections.Generic;

namespace Deislabs.Bindle;

// We can't use init properties here because we can't guarantee that
// they will be initialised to non-null values; so we have to write an
// explicit constructor (and make defensive copies).
public class Invoice
{
    public string BindleVersion { get; set; } = "";
    public bool Yanked { get; set; }
    public BindleMetadata? Bindle { get; set; }
    public IDictionary<string, string> Annotations { get; set; } = new Dictionary<string, string>();
    public IList<Parcel> Parcels { get; set; } = new List<Parcel>();
    public IList<Group> Groups { get; set; } = new List<Group>();
}

public class BindleMetadata
{
    public string? Name { get; set; }
    public string? Version { get; set; }
    public string? Description { get; set; }
    public IList<string> Authors { get; set; } = new List<string>();
}

public class Parcel
{
    public Label? Label { get; set; }
    public Conditions? Conditions { get; set; }
}

public class Label
{
    public string? Name { get; set; }
    public string? Sha256 { get; set; }
    public string? MediaType { get; set; }
    public long Size { get; set; }
    public IDictionary<string, string> Annotations { get; set; } = new Dictionary<string, string>();
    public IDictionary<string, IDictionary<string, string>> Feature { get; set; } = new Dictionary<string, IDictionary<string, string>>();
}

public class Conditions
{
    public IList<string> MemberOf { get; set; } = new List<string>();
    public IList<string> Requires { get; set; } = new List<string>();
}

public class Group
{
    public string? Name { get; set; }
    public bool Required { get; set; }
    public SatisfiedBy SatisfiedBy { get; set; }
}

public enum SatisfiedBy
{
    AllOf,
    OneOf,
    Optional,
}

public class CreateInvoiceResult
{
    public Invoice Invoice { get; set; } = new Invoice { };
    public IList<Label> MissingParcels { get; set; } = new List<Label>();
}
