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

        private readonly Uri _baseUri;

        public async Task<Invoice> GetInvoice(string invoiceId)
        {
            var httpClient = new HttpClient();
            var uri = new Uri(_baseUri, "_i/" + invoiceId);
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

        private static string SlashSafe(string uri)
        {
            if (uri.EndsWith('/'))
            {
                return uri;
            }
            return uri + '/';
        }

        private async static Task<TomlTable> ReadResponseToml(HttpResponseMessage response) {
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
    }
}
