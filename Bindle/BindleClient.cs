using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Tomlyn.Model;

namespace Bindle
{
    public class BindleClient
    {
        public BindleClient(
            string baseUri
        )
        {
            _baseUri = new Uri(SlashSafe(baseUri));
        }

        private const string INVOICE_PATH = "_i/";
        private readonly Uri _baseUri;

        public async Task<Invoice> GetInvoice(string invoiceId, GetInvoiceOptions options = GetInvoiceOptions.None)
        {
            var query = GetInvoiceQueryString(options);
            var httpClient = new HttpClient();
            var uri = new Uri(_baseUri, INVOICE_PATH + invoiceId + query);
            var response = await httpClient.GetAsync(uri);
            if (response == null)
            {
                throw new Exception("No response from Bindle server");
            }
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new System.Net.WebException($"Bindle server returned status code {response.StatusCode}");
            }
            var toml = await ReadResponseToml(response);
            return Parser.ParseInvoice(toml);
        }

        public async Task<CreateInvoiceResult> CreateInvoice(Invoice invoice)
        {
            TomlTable invoiceModel = TomliseInvoice(invoice);
            var invoiceToml = invoiceModel.ToString();

            if (invoiceToml == null) {
                throw new Exception("Error serialising invoice to TOML");
            }

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Content-Type", "application/toml");
            var uri = new Uri(_baseUri, INVOICE_PATH);
            var response = await httpClient.PostAsync(uri, new StringContent(invoiceToml));
        }

        private static string SlashSafe(string uri)
        {
            if (uri.EndsWith('/'))
            {
                return uri;
            }
            return uri + '/';
        }

        private async static Task<TomlTable> ReadResponseToml(HttpResponseMessage response)
        {
            var responseText = await response.Content.ReadAsStringAsync();
            var responseToml = Tomlyn.Toml.Parse(responseText);
            if (responseToml == null)
            {
                throw new Exception("Empty response from Bindle server");
            }
            if (responseToml.HasErrors)
            {
                var errors = String.Join(", ", responseToml.Diagnostics.Select(d => d.Message));
                throw new Exception($"Invalid response from Bindle server: {errors}");
            }
            return Tomlyn.Toml.ToModel(responseToml);
        }

        private static string GetInvoiceQueryString(GetInvoiceOptions options)
        {
            if ((options & GetInvoiceOptions.IncludeYanked) == GetInvoiceOptions.IncludeYanked)
            {
                return "?yanked=true";
            }
            return String.Empty;
        }
    }
}
