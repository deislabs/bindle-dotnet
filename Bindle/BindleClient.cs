using System;
using System.Collections.Generic;
using System.IO;
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
            _httpClient = new HttpClient();
        }

        public BindleClient(
            string baseUri,
            HttpMessageHandler messageHandler
        )
        {
            _baseUri = new Uri(SlashSafe(baseUri));
            _httpClient = new HttpClient(messageHandler);
        }

        private const string INVOICE_PATH = "_i";
        private const string QUERY_PATH = "_q";
        private const string RELATIONSHIP_PATH = "_r";

        private readonly Uri _baseUri;
        private readonly HttpClient _httpClient;

        public async Task<Invoice> GetInvoice(string invoiceId, GetInvoiceOptions options = GetInvoiceOptions.None)
        {
            var query = GetInvoiceQueryString(options);
            var uri = new Uri(_baseUri, $"{INVOICE_PATH}/{invoiceId}{query}");
            var response = await _httpClient.GetAsync(uri);
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
            var invoiceToml = InvoiceWriter.Write(invoice);

            if (invoiceToml == null)
            {
                throw new Exception("Error serialising invoice to TOML");
            }

            var uri = new Uri(_baseUri, INVOICE_PATH);
            var requestContent = new StringContent(invoiceToml, null, "application/toml");
            if (requestContent.Headers.ContentType != null)
            {
                requestContent.Headers.ContentType.CharSet = null;  // The Bindle server is VERY strict about the contents of the Content-Type header
            }
            var response = await _httpClient.PostAsync(uri, requestContent);

            if (response == null)
            {
                throw new Exception("No response from Bindle server");
            }
            if (response.StatusCode != HttpStatusCode.Accepted && response.StatusCode != HttpStatusCode.Created)
            {
                throw new System.Net.WebException($"Bindle server returned status code {response.StatusCode}");
            }
            var toml = await ReadResponseToml(response);
            return Parser.ParseCreateInvoiceResult(toml);
        }

        public async Task YankInvoice(string invoiceId)
        {
            var uri = new Uri(_baseUri, $"{INVOICE_PATH}/{invoiceId}");
            await _httpClient.DeleteAsync(uri);
        }

        public async Task<HttpContent> GetParcel(string invoiceId, string parcelId)
        {
            var uri = new Uri(_baseUri, $"{INVOICE_PATH}/{invoiceId}@{parcelId}");
            var response = await _httpClient.GetAsync(uri);
            if (response == null)
            {
                throw new Exception("No response from Bindle server");
            }
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new System.Net.WebException($"Bindle server returned status code {response.StatusCode}");
            }
            return response.Content;
        }

        public async Task CreateParcel(string invoiceId, string parcelId, Stream content)
        {
            await CreateParcel(invoiceId, parcelId, new StreamContent(content));
        }

        public async Task CreateParcel(string invoiceId, string parcelId, string content)
        {
            await CreateParcel(invoiceId, parcelId, new StringContent(content));
        }

        public async Task CreateParcel(string invoiceId, string parcelId, byte[] content)
        {
            await CreateParcel(invoiceId, parcelId, new ByteArrayContent(content));
        }

        public async Task CreateParcel(string invoiceId, string parcelId, HttpContent content)
        {
            var uri = new Uri(_baseUri, $"{INVOICE_PATH}/{invoiceId}@{parcelId}");
            await _httpClient.PostAsync(uri, content);
        }

        public async Task<IEnumerable<Label>> ListMissingParcels(string invoiceId)
        {
            var uri = new Uri(_baseUri, $"{RELATIONSHIP_PATH}/missing/{invoiceId}");
            var response = await _httpClient.GetAsync(uri);
            if (response == null)
            {
                throw new Exception("No response from Bindle server");
            }
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new System.Net.WebException($"Bindle server returned status code {response.StatusCode}");
            }
            var toml = await ReadResponseToml(response);
            return Parser.ParseMissingLabels(toml);
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
