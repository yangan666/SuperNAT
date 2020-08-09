using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNAT.AsyncSocket
{
    public class SessionContainer<TSession> where TSession : ISession, new()
    {
        private static readonly object lockObj = new object();
        public List<TSession> SessionList { get; set; } = new List<TSession>();

        public long SessionCount => SessionList.Count;

        public void Add(TSession session)
        {
            lock (lockObj)
            {
                SessionList.Add(session);
            }
        }

        public void Remove(TSession session)
        {
            lock (lockObj)
            {
                SessionList.RemoveAll(c => c.SessionId == session.SessionId);
            }
        }

        public void Remove(string sessionId)
        {
            lock (lockObj)
            {
                SessionList.RemoveAll(c => c.SessionId == sessionId);
            }
        }
    }
}
