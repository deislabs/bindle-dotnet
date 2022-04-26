using System.Collections.Generic;

namespace Deislabs.Bindle;

// We can't use init properties here because we can't guarantee that
// they will be initialised to non-null values; so we have to write an
// explicit constructor (and make defensive copies).
public class Invoice
{
    public Invoice() { }

    public Invoice(
        string bindleVersion,
        bool yanked,
        BindleMetadata bindle,
        IDictionary<string, string> annotations,
        IEnumerable<Parcel> parcels,
        IEnumerable<Group> groups
    )
    {
        BindleVersion = bindleVersion;
        Yanked = yanked;
        Bindle = bindle;
        Annotations = DefensiveCopy.Create(annotations);
        Parcels = DefensiveCopy.Create(parcels);
        Groups = DefensiveCopy.Create(groups);
    }

    public string BindleVersion { get; } = "";
    public bool Yanked { get; }
    public BindleMetadata Bindle { get; } = new BindleMetadata();
    public IReadOnlyDictionary<string, string> Annotations { get; } = new Dictionary<string, string>();
    public IReadOnlyList<Parcel> Parcels { get; } = new List<Parcel>();
    public IReadOnlyList<Group> Groups { get; } = new List<Group>();
}

public class BindleMetadata
{
    public BindleMetadata() { }

    public BindleMetadata(string name, string version)
    {
        Name = name;
        Version = version;
    }

    public BindleMetadata(
        string name,
        string version,
        string? description,
        IEnumerable<string> authors
    )
    {
        Name = name;
        Version = version;
        Description = description;
        Authors = DefensiveCopy.Create(authors);
    }

    public string? Name { get; }
    public string? Version { get; }
    public string? Description { get; }
    public IReadOnlyList<string> Authors { get; } = new List<string>();
}

public class Parcel
{
    public Parcel(
        Label label,
        Conditions? conditions
    )
    {
        Label = label;
        Conditions = conditions;
    }

    public Label Label { get; }
    public Conditions? Conditions { get; }
}

public class Label
{
    public Label(
        string name,
        string sha256,
        string mediaType,
        long size,
        IDictionary<string, string> annotations,
        IDictionary<string, IDictionary<string, string>> feature
    )
    {
        Name = name;
        Sha256 = sha256;
        MediaType = mediaType;
        Size = size;
        Annotations = DefensiveCopy.Create(annotations);
        Feature = DefensiveCopy.Create(feature);
    }

    public string Name { get; }
    public string Sha256 { get; }
    public string MediaType { get; }
    public long Size { get; }
    public IReadOnlyDictionary<string, string> Annotations { get; }
    public IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> Feature { get; }
}

public class Conditions
{
    public Conditions(
        IEnumerable<string> memberOf,
        IEnumerable<string> requires
    )
    {
        MemberOf = DefensiveCopy.Create(memberOf);
        Requires = DefensiveCopy.Create(requires);
    }

    public IReadOnlyList<string> MemberOf { get; }
    public IReadOnlyList<string> Requires { get; }
}

public class Group
{
    public Group(
        string name,
        bool required,
        SatisfiedBy satisfiedBy
    )
    {
        Name = name;
        Required = required;
        SatisfiedBy = satisfiedBy;
    }

    public string Name { get; }
    public bool Required { get; }
    public SatisfiedBy SatisfiedBy { get; }
}

public enum SatisfiedBy
{
    AllOf,
    OneOf,
    Optional,
}

public class CreateInvoiceResult
{
    public CreateInvoiceResult() { }

    public CreateInvoiceResult(
        Invoice invoice,
        IEnumerable<Label> missingParcels
    )
    {
        Invoice = invoice;
        MissingParcels = DefensiveCopy.Create(missingParcels);
    }

    public Invoice Invoice { get; } = new Invoice { };
    public IReadOnlyList<Label> MissingParcels { get; } = new List<Label>();
}
