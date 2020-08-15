using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.AsyncSocket
{
    public interface IServer<TSession, TRequestInfo>
        where TSession : ISession, new()
        where TRequestInfo : IRequestInfo, new()
    {
        ServerOption ServerOption { get; set; }
        Socket Socket { get; set; }
        string ServerId { get; set; }
        DateTime StartTime { get; set; }
        Action<TSession> OnConnected { get; set; }
        Action<TSession, TRequestInfo> OnReceived { get; set; }
        Action<TSession> OnClosed { get; set; }
        Task<bool> StartAysnc();
        void Stop();
        SessionContainer<TSession> SessionContainer { get; set; }
        IReceiveFilter<TRequestInfo> NextReceiveFilter { get; set; }
        List<TSession> GetSessionList(Predicate<TSession> predicate = null);
        TSession GetSingleSession(Predicate<TSession> predicate);
        long SessionCount { get; }
    }
}
