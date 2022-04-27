using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static Deislabs.Bindle.GetInvoiceOptions;

namespace Deislabs.Bindle.IntegrationTests;

// To run this against the data assumed in the integration tests, run the Bindle server
// to serve files on port 14044 from the test/data directory.  If you have Rust installed you can
// do by cloning the Bindle repo and running:
//
// RUST_LOG=info cargo run --bin bindle-server --features="cli" -- -i 127.0.0.1:14044 -d <this_repo>/Bindle.Tests/data

// Alternatively if you set the variable BINDLE_SERVER_PATH to the relative path of bindle-server then the test will start and stop the server automatically
// e.g. if bindle is in a peer directory to bindle-dotnet and the debug build is being used then set the variable to "../../../../../bindle/target/debug"

[TestCaseOrderer("Deislabs.Bindle.Tests.TestPriorityOrderer", "Bindle.Tests")]
public class Integration : IClassFixture<IntegrationFixture>
{
    const string DEMO_SERVER_URL = "http://localhost:14044/v1";

    [Fact]
    public async Task CanFetchInvoice()
    {
        var client = new BindleClient(DEMO_SERVER_URL);
        var invoice = await client.GetInvoice("your/fancy/bindle/0.3.0");
        Assert.Equal("1.0.0", invoice.BindleVersion);
        Assert.Equal("your/fancy/bindle", invoice.Bindle.Name);
        Assert.Equal("main", invoice.Annotations["engineering_location"]);
        Assert.Equal(4, invoice.Parcels.Count);
        Assert.Equal("daemon", invoice.Parcels[0].Label.Name);
        Assert.Equal(3, invoice.Groups.Count);
        Assert.Equal("server", invoice.Groups[0].Name);
    }

    [Fact]
    public async Task CanQueryInvoices()
    {
        var client = new BindleClient(DEMO_SERVER_URL);
        var invoices = await client.QueryInvoices(queryString: "my");
        Assert.True(invoices.All(i => i.Bindle.Name.Contains("my")));
    }

    [Fact]
    public async Task CanFetchYankedInvoices()
    {
        var client = new BindleClient(DEMO_SERVER_URL);
        var invoice = await client.GetInvoice("yourbindle/0.1.1", IncludeYanked);
        Assert.Equal("1.0.0", invoice.BindleVersion);
        Assert.Equal("yourbindle", invoice.Bindle.Name);
        Assert.Equal(3, invoice.Parcels.Count);
    }

    [Fact]
    public async Task CanCreateInvoices()
    {
        var client = new BindleClient(DEMO_SERVER_URL);
        var invoice = new Invoice
        {
            BindleVersion = "1.0.0",
            Bindle = new BindleMetadata
            {
                Name = "bernards/abominable/bindle",
                Version = "0.0.1",
                Description = "an abominable bindle",
                Authors = new[] { "some chap named Bernard" }
            },
            Annotations = new Dictionary<string, string> {
                    { "penguinType", "adelie" }
            },
            Parcels = new[] {
                    new Parcel
                    {
                        Label = new Label
                        {
                            Name = "gary",
                            Sha256 = "f7f3b33707fb76d208f5839a40e770452dcf9f348bfd7faf2c524e0fa6710ed6",
                            MediaType = "text/plain",
                            Size = 15,
                            Annotations = new Dictionary<string, string>(),
                            Feature = new Dictionary<string, IDictionary<string, string>>()
                        }
                    },
                    new Parcel
                    {
                        Label = new Label
                        {
                            Name = "keith",
                            Sha256 = "45678",
                            MediaType = "text/plain",
                            Size = 20,
                            Annotations = new Dictionary<string, string>(),
                            Feature = new Dictionary<string, IDictionary<string, string>> {
                                { "test", new Dictionary<string, string> {
                                    { "a", "1" },
                                    { "b", "2" },
                                }}
                            }
                        },
                    },
            },
            Groups = new[] {
                new Group{Name = "group1", Required = true, SatisfiedBy = SatisfiedBy.AllOf}
            }
        };
        var createResult = await client.CreateInvoice(invoice);
        Assert.Equal(1, createResult.MissingParcels.Count);
        var fetched = await client.GetInvoice("bernards/abominable/bindle/0.0.1");
        Assert.Equal(invoice.Annotations["penguinType"], fetched.Annotations["penguinType"]);
        Assert.Equal(invoice.Parcels.Count, fetched.Parcels.Count);
        Assert.Equal(invoice.Parcels[1].Label.Feature["test"]["a"], fetched.Parcels[1].Label.Feature["test"]["a"]);
        Assert.Equal(invoice.Groups.Count, fetched.Groups.Count);
    }

    // Make sure CanYankInvoice runs after CanCreateInvoice
    [Fact]
    [TestPriority(10)]
    public async Task CanYankInvoice()
    {
        var client = new BindleClient(DEMO_SERVER_URL);
        await client.YankInvoice("your/fancy/bindle/0.3.0");
        await Assert.ThrowsAsync<BindleYankedException>(async () =>
        {
            await client.GetInvoice("your/fancy/bindle/0.3.0");
        });
        var invoice = await client.GetInvoice("your/fancy/bindle/0.3.0", IncludeYanked);
        Assert.Equal("your/fancy/bindle", invoice.Bindle.Name);
    }

    [Fact]
    public async Task CanFetchParcel()
    {
        var client = new BindleClient(DEMO_SERVER_URL);
        var parcel = await client.GetParcel("mybindle/0.1.0", "f7f3b33707fb76d208f5839a40e770452dcf9f348bfd7faf2c524e0fa6710ed6");
        Assert.Equal("Fie on you Gary", await parcel.ReadAsStringAsync());
    }

    [Fact]
    public async Task CanCreateParcel()
    {
        var client = new BindleClient(DEMO_SERVER_URL);
        await client.CreateParcel("mybindle/0.1.0", "460d5965e4d1909e8c7a3748a414956b7038ab5fd79937c9fcb2b214e6b0160a", "The front fell off");
        var fetched = await client.GetParcel("mybindle/0.1.0", "460d5965e4d1909e8c7a3748a414956b7038ab5fd79937c9fcb2b214e6b0160a");
        Assert.Equal("The front fell off", await fetched.ReadAsStringAsync());
    }

    [Fact]
    public async Task CanListMissingParcels()
    {
        var client = new BindleClient(DEMO_SERVER_URL);
        var missing = await client.ListMissingParcels("mybindle/0.3.0");
        Assert.Contains(missing, (label) => label.Sha256 == "e1706ab0a39ac88094b6d54a3f5cdba41fe5a901");
        Assert.DoesNotContain(missing, (label) => label.Sha256 == "f7f3b33707fb76d208f5839a40e770452dcf9f348bfd7faf2c524e0fa6710ed6");
    }
}
