using System;

namespace Deislabs.Bindle
{
    [Flags]
    public enum GetInvoiceOptions
    {
        None = 0,
        IncludeYanked = 1,
    }
}
