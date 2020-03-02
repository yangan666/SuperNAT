using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNAT.Model
{
    public enum protocol
    {
        http,
        https,
        tcp,
        udp
    }

    public enum ssl_type
    {
        ssl3 = 0x30,
        tls = 0xC0,
        tls11 = 0x300,
        tls12 = 0xC00,
        tls13 = 0x3000
    }

    public enum ChangeMapType
    {
        新增 = 1,
        修改 = 2,
        删除 = 3
    }

    public enum proxy_type
    {
        反向代理 = 1,
        正向代理 = 2
    }
}
