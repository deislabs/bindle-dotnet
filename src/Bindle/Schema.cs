using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Deislabs.Bindle;

// We can't use init properties here because we can't guarantee that
// they will be initialised to non-null values; so we have to write an
// explicit constructor (and make defensive copies).
public class Invoice
{
    public string BindleVersion { get; set; } = "";
    public bool Yanked { get; set; }
    public BindleMetadata? Bindle { get; set; }
    public Dictionary<string, string> Annotations { get; set; } = new Dictionary<string, string>();

    // TODO(bacongobbler): upstream naming bug?
    [DataMember(Name = "signature")]
    public List<Signature> Signatures { get; set; } = new List<Signature>();

    // TODO(bacongobbler): upstream naming bug?
    [DataMember(Name = "parcel")]
    public List<Parcel> Parcels { get; set; } = new List<Parcel>();

    // TODO(bacongobbler): upstream naming bug?
    [DataMember(Name = "group")]
    public List<Group> Groups { get; set; } = new List<Group>();
}

public class BindleMetadata
{
    public string? Name { get; set; }
    public string? Version { get; set; }
    public string? Description { get; set; }
    public List<string> Authors { get; set; } = new List<string>();
}

public class Signature
{
    [DataMember(Name = "signature")]
    public string? Sig { get; set; }
    public string? By { get; set; }
    public string? Key { get; set; }
    public string? Role { get; set; }
    public long At { get; set; }
}

public class SignatureKey
{
    public string? Label { get; set; }
    public List<string> Roles { get; set; } = new List<string>();
    public string? Key { get; set; }
    public string? LabelSignature { get; set; }
}

public class Keyring
{
    public string? Version { get; set; }
    public List<SignatureKey> Key { get; set; } = new List<SignatureKey>();
}

public class Parcel
{
    public Label? Label { get; set; }
    public Condition? Conditions { get; set; }
}

public class Label
{
    public string? Name { get; set; }
    public string? Sha256 { get; set; }
    public string? MediaType { get; set; }
    public long Size { get; set; }
    public Dictionary<string, string> Annotations { get; set; } = new Dictionary<string, string>();
    public Dictionary<string, Dictionary<string, string>> Feature { get; set; } = new Dictionary<string, Dictionary<string, string>>();
}

public class Condition
{
    public List<string> MemberOf { get; set; } = new List<string>();
    public List<string> Requires { get; set; } = new List<string>();
}

public class Group
{
    public string? Name { get; set; }
    public bool Required { get; set; }
    public string? SatisfiedBy { get; set; }
}

public class CreateInvoiceResult
{
    public Invoice Invoice { get; set; } = new Invoice { };
    public List<Label> Missing { get; set; } = new List<Label>();
}

/// Describes the matches that are returned from a query
public class Matches
{
    public string? Query { get; set; }
    public bool Strict { get; set; }
    public long Offset { get; set; }
    public long Limit { get; set; }
    public long Total { get; set; }
    public bool More { get; set; }
    public bool Yanked { get; set; }
    public List<Invoice> Invoices { get; set; } = new List<Invoice>();
}

public class MissingParcelsResponse
{
    public List<Label> Missing { get; set; } = new List<Label>();
}
