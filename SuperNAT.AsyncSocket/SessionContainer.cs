using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNAT.AsyncSocket
{
    /// <summary>
    /// Session容器
    /// </summary>
    /// <typeparam name="TSession"></typeparam>
    public class SessionContainer<TSession> where TSession : ISession, new()
    {
        /// <summary>
        /// 读写锁
        /// </summary>
        private static readonly object lockObj = new object();

        /// <summary>
        /// Session集合
        /// </summary>
        public List<TSession> SessionList { get; set; } = new List<TSession>();

        /// <summary>
        /// 会话数
        /// </summary>
        public long SessionCount => SessionList.Count;

        /// <summary>
        /// 添加一个Session到容器
        /// </summary>
        /// <param name="session"></param>
        public void Add(TSession session)
        {
            lock (lockObj)
            {
                SessionList.Add(session);
            }
        }

        /// <summary>
        /// 从容器移除一个Session
        /// </summary>
        /// <param name="session">连接会话</param>
        public void Remove(TSession session)
        {
            lock (lockObj)
            {
                SessionList.RemoveAll(c => c.SessionId == session.SessionId);
            }
        }

        /// <summary>
        /// 从容器移除一个Session
        /// </summary>
        /// <param name="sessionId">会话ID</param>
        public void Remove(string sessionId)
        {
            lock (lockObj)
            {
                SessionList.RemoveAll(c => c.SessionId == sessionId);
            }
        }
    }
}
