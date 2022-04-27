using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Tomlyn;
using Tomlyn.Syntax;

namespace Deislabs.Bindle;

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
        await ExpectResponseCode(response, HttpStatusCode.OK, HttpStatusCode.Forbidden);

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            if ((options & GetInvoiceOptions.IncludeYanked) == 0)
            {
                throw new BindleYankedException();
            }
            else
            {
                throw new BindleProtocolException($"Bindle server returned status code {response.StatusCode}", response);
            }
        }

        var content = await response.Content.ReadAsStringAsync();
        var syntax = GetTomlSyntax(content);
        var tomlOptions = new TomlModelOptions()
        {
            ConvertPropertyName = name => TomlNamingHelper.PascalToCamelCase(name)
        };
        return syntax.ToModel<Invoice>(tomlOptions);
    }

    public async Task<Matches> QueryInvoices(string? queryString = null,
        ulong? offset = null,
        long? limit = null,
        bool? strict = null,
        string? semVer = null,
        bool? yanked = null)
    {
        var query = GetDistinctInvoicesNamesQueryString(queryString, offset, limit, strict, semVer, yanked);
        var uri = new Uri(_baseUri, $"{QUERY_PATH}?{query}");
        var response = await _httpClient.GetAsync(uri);
        await ExpectResponseCode(response, HttpStatusCode.OK, HttpStatusCode.Forbidden);

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new BindleProtocolException($"Bindle server returned status code {response.StatusCode}", response);
        }

        var content = await response.Content.ReadAsStringAsync();
        var syntax = GetTomlSyntax(content);
        var tomlOptions = new TomlModelOptions()
        {
            ConvertPropertyName = name => TomlNamingHelper.PascalToCamelCase(name)
        };
        return syntax.ToModel<Matches>(tomlOptions);
    }

    public async Task<CreateInvoiceResult> CreateInvoice(Invoice invoice)
    {
        var invoiceToml = Toml.FromModel(invoice, new TomlModelOptions()
        {
            ConvertPropertyName = name => TomlNamingHelper.PascalToCamelCase(name)
        });

        var uri = new Uri(_baseUri, INVOICE_PATH);
        var requestContent = new StringContent(invoiceToml, null, "application/toml");
        var response = await _httpClient.PostAsync(uri, requestContent);
        await ExpectResponseCode(response, HttpStatusCode.Created, HttpStatusCode.Accepted);

        var content = await response.Content.ReadAsStringAsync();
        var syntax = GetTomlSyntax(content);
        var tomlOptions = new TomlModelOptions()
        {
            ConvertPropertyName = name => TomlNamingHelper.PascalToCamelCase(name)
        };
        return syntax.ToModel<CreateInvoiceResult>(tomlOptions);
    }

    public async Task YankInvoice(string invoiceId)
    {
        var uri = new Uri(_baseUri, $"{INVOICE_PATH}/{invoiceId}");
        var response = await _httpClient.DeleteAsync(uri);
        await ExpectResponseCode(response, HttpStatusCode.OK);
    }

    public async Task<HttpContent> GetParcel(string invoiceId, string parcelId)
    {
        var uri = new Uri(_baseUri, $"{INVOICE_PATH}/{invoiceId}@{parcelId}");
        var response = await _httpClient.GetAsync(uri);
        await ExpectResponseCode(response, HttpStatusCode.OK);

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
        var response = await _httpClient.PostAsync(uri, content);
        await ExpectResponseCode(response, HttpStatusCode.OK, HttpStatusCode.Created);
    }

    public async Task<MissingParcelsResponse> ListMissingParcels(string invoiceId)
    {
        var uri = new Uri(_baseUri, $"{RELATIONSHIP_PATH}/missing/{invoiceId}");
        var response = await _httpClient.GetAsync(uri);
        await ExpectResponseCode(response, HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var syntax = GetTomlSyntax(content);
        var tomlOptions = new TomlModelOptions()
        {
            ConvertPropertyName = name => TomlNamingHelper.PascalToCamelCase(name)
        };
        return syntax.ToModel<MissingParcelsResponse>(tomlOptions);
    }

    private static string SlashSafe(string uri)
    {
        if (uri.EndsWith('/'))
        {
            return uri;
        }
        return uri + '/';
    }

    private async static Task ExpectResponseCode(HttpResponseMessage response, params HttpStatusCode[] codes)
    {
        _ = response ?? throw new NoResponseException();

        if (!codes.Contains(response.StatusCode))
        {
            throw new BindleProtocolException($"Bindle server returned status code {response.StatusCode}: {await response.Content.ReadAsStringAsync()}", response);
        }
    }

    private static DocumentSyntax GetTomlSyntax(string content)
    {
        var responseToml = Toml.Parse(content);
        _ = responseToml ?? throw new ResponseContentException("Empty response from Bindle server");

        if (responseToml.HasErrors)
        {
            var errors = String.Join(", ", responseToml.Diagnostics.Select(d => d.Message));
            throw new ResponseContentException($"Invalid response from Bindle server: {errors}");
        }
        return responseToml;
    }

    private static string GetInvoiceQueryString(GetInvoiceOptions options)
    {
        if ((options & GetInvoiceOptions.IncludeYanked) == GetInvoiceOptions.IncludeYanked)
        {
            return "?yanked=true";
        }
        return String.Empty;
    }

    private static string GetDistinctInvoicesNamesQueryString(string? queryString = null,
        ulong? offset = null,
        long? limit = null,
        bool? strict = false,
        string? semVer = null,
        bool? yanked = null)
    {
        NameValueCollection query = System.Web.HttpUtility.ParseQueryString(string.Empty);

        if (!String.IsNullOrEmpty(queryString))
        {
            query.Add("q", queryString);
        }
        if (offset.HasValue)
        {
            query.Add("o", offset.Value.ToString());
        }
        if (limit.HasValue)
        {
            query.Add("l", limit.Value.ToString());
        }
        if (strict.HasValue)
        {
            query.Add("strict", strict.Value.ToString().ToLower());
        }
        if (!String.IsNullOrEmpty(semVer))
        {
            query.Add("v", semVer);
        }
        if (yanked.HasValue)
        {
            query.Add("yanked", yanked.Value.ToString().ToLower());
        }

        return query.ToString() ?? "";
    }
}
