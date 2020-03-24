using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.AsyncSocket
{
    public interface IServer
    {
        bool Start();

        void Stop();

        long SessionCount { get; }
    }
}
