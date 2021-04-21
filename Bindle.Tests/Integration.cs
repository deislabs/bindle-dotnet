using System;
using System.Threading.Tasks;
using Xunit;

namespace Bindle.Tests
{
    // To run this against the data assumed in the integration tests, run the Bindle server
    // to serve files on port 14044 from the test/data directory.  If you have Rust installed you can
    // do by cloning the Bindle repo and running:
    //
    // RUST_LOG=info cargo run --bin bindle-server --features="cli" -- -i 127.0.0.1:14044 -d <this_repo>/test/data

    public class Integration
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
    }
}
