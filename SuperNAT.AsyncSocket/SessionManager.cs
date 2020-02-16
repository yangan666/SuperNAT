using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperNAT.AsyncSocket
{
    public class SessionManager<TSession> where TSession : ISession, new()
    {
        private static object lockObj = new object();
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
    }
}
