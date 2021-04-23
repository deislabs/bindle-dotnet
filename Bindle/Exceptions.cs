using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Bindle
{
    public class BindleProtocolException: Exception
    {
        public BindleProtocolException(string message, HttpResponseMessage response): base(message)
        {
            Response = response;
        }

        public HttpResponseMessage Response { get; }

        public HttpStatusCode StatusCode => Response.StatusCode;
        public async Task<string?> ResponseText() => await Response.Content.ReadAsStringAsync();
    }

    public class ResponseContentException: Exception
    {
        public ResponseContentException(string message): base(message)
        {
        }
    }

    public class BindleYankedException: Exception
    {
        public BindleYankedException(): base("Server returned Forbidden; bindle may have been yanked")
        {
        }
    }

    public class NoResponseException: Exception
    {
        public NoResponseException(): base("No response from Bindle server")
        {
        }
    }
}