using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.AsyncSocket
{
    /// <summary>
    /// 服务端接口
    /// </summary>
    /// <typeparam name="TSession"></typeparam>
    /// <typeparam name="TRequestInfo"></typeparam>
    public interface IServer<TSession, TRequestInfo>
        where TSession : ISession, new()
        where TRequestInfo : IRequestInfo, new()
    {
        /// <summary>
        /// 服务端配置
        /// </summary>
        ServerOption ServerOption { get; set; }
        /// <summary>
        /// 服务端Socket
        /// </summary>
        Socket Socket { get; set; }
        /// <summary>
        /// 服务ID
        /// </summary>
        string ServerId { get; set; }
        /// <summary>
        /// 服务启动时间
        /// </summary>
        DateTime StartTime { get; set; }
        /// <summary>
        /// 连接事件
        /// </summary>
        Action<TSession> OnConnected { get; set; }
        /// <summary>
        /// 接收数据事件
        /// </summary>
        Action<TSession, TRequestInfo> OnReceived { get; set; }
        /// <summary>
        /// 关闭事件
        /// </summary>
        Action<TSession> OnClosed { get; set; }
        /// <summary>
        /// 启动服务
        /// </summary>
        /// <returns></returns>
        Task<bool> StartAysnc();
        /// <summary>
        /// 停止服务
        /// </summary>
        void Stop();
        /// <summary>
        /// Session容器
        /// </summary>
        SessionContainer<TSession> SessionContainer { get; set; }
        /// <summary>
        /// 下一个过滤器
        /// </summary>
        IReceiveFilter<TRequestInfo> NextReceiveFilter { get; set; }
        /// <summary>
        /// 获取Session列表
        /// </summary>
        /// <param name="predicate">查询条件</param>
        List<TSession> GetSessionList(Predicate<TSession> predicate = null);
        /// <summary>
        /// 获取单个Session
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns></returns>
        TSession GetSingleSession(Predicate<TSession> predicate);
        /// <summary>
        /// 连接数
        /// </summary>
        long SessionCount { get; }
    }
}
