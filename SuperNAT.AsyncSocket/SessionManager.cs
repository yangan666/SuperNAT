using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperNAT.AsyncSocket
{
    public class SessionManager
    {
        private static object lockObj = new object();
        public List<AsyncUserToken> SessionList { get; set; } = new List<AsyncUserToken>();

        public long SessionCount => SessionList.Count;

        public void Add(AsyncUserToken token)
        {
            lock (lockObj)
            {
                SessionList.Add(token);
            }
        }

        public void Remove(AsyncUserToken token)
        {
            lock (lockObj)
            {
                SessionList.Remove(token);
            }
        }

        public AsyncUserToken GetSingle(Func<AsyncUserToken, bool> predicate)
        {
            return SessionList.FirstOrDefault(predicate);
        }
    }
}
