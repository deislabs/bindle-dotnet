using System;
using System.Threading.Tasks;
using Xunit;

namespace Bindle.Tests
{
    public class Integration
    {
        [Fact]
        public async Task CanFetchInvoice()
        {
            var client = new BindleClient();
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
