using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Deislabs.Bindle
{
	public interface IBindleClient
	{
		Task<CreateInvoiceResult> CreateInvoice(Invoice invoice);
		Task CreateParcel(string invoiceId, string parcelId, byte[] content);
		Task CreateParcel(string invoiceId, string parcelId, HttpContent content);
		Task CreateParcel(string invoiceId, string parcelId, Stream content);
		Task CreateParcel(string invoiceId, string parcelId, string content);
		Task<Invoice> GetInvoice(string invoiceId, GetInvoiceOptions options = GetInvoiceOptions.None);
		Task<HttpContent> GetParcel(string invoiceId, string parcelId);
		Task<MissingParcelsResponse> ListMissingParcels(string invoiceId);
		Task<Matches> QueryInvoices(string? queryString = null, ulong? offset = null, long? limit = null, bool? strict = null, string? semVer = null, bool? yanked = null);
		Task YankInvoice(string invoiceId);
	}
}