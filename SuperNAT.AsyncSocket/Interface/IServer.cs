using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.AsyncSocket
{
    public interface IServer
    {
        Task StartAsync();

        void Stop();

        long SessionCount { get; }
    }
}
