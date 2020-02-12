using SuperNAT.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNAT.Server
{
    public interface IServer
    {
        void Start();
        void Stop();
        void Response(PackJson packJson);
    }
}
