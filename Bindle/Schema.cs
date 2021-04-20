using System.Collections.Generic;
using System.Linq;

namespace Bindle
{
    // We can't use init properties here because we can't guarantee that
    // they will be initialised to non-null values; so we have to write an
    // explicit constructor (and make defensive copies).

    public class Invoice
    {
        internal Invoice(
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
            Annotations = new Dictionary<string, string>(annotations);
            Parcels = new List<Parcel>(parcels).AsReadOnly();
            Groups = new List<Group>(groups).AsReadOnly();
        }

        public string BindleVersion { get; }
        public bool Yanked { get; }
        public BindleMetadata Bindle { get; }
        public IReadOnlyDictionary<string, string> Annotations { get; }
        public IReadOnlyList<Parcel> Parcels { get; }
        public IReadOnlyList<Group> Groups { get; }
    }

    public class BindleMetadata
    {
        public string Name { get; init; }
        public string Version { get; init; }
        public string? Description { get; init; }
        public IReadOnlyList<string> Authors { get; init; }
    }

    public class Parcel
    {

    }

    public class Group
    {

    }
}