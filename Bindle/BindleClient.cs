using System;
using System.Threading.Tasks;

namespace Bindle
{
    public class BindleClient
    {
        public async Task<Invoice> GetInvoice(string invoiceId)
        {
            await Task.Yield();
            throw new NotImplementedException();
        }
    }
}
