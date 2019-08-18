using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNAT.Common.Models
{
    public enum ssl_type
    {
        ssl3 = 0x30,
        tls = 0xC0,
        tls11 = 0x300,
        tls12 = 0xC00,
        tls13 = 0x3000
    }
}
