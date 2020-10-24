using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuperNAT.AsyncSocket
{
    /// <summary>
    /// Socket服务
    /// </summary>
    /// <typeparam name="TSession"></typeparam>
    /// <typeparam name="TRequestInfo"></typeparam>
    public abstract class AppServer<TSession, TRequestInfo> : IServer<TSession, TRequestInfo>
        where TSession : ISession, new()
        where TRequestInfo : IRequestInfo, new()
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="receiveFilter">过滤器</param>
        public AppServer(IReceiveFilter<TRequestInfo> receiveFilter)
        {
            NextReceiveFilter = receiveFilter;
        }
        /// <summary>
        /// 初始化服务端配置
        /// </summary>
        /// <param name="serverOption">服务端配置</param>
        public void InitOption(ServerOption serverOption)
        {
            ServerOption = serverOption;
        }
        /// <summary>
        /// 服务端配置
        /// </summary>
        public ServerOption ServerOption { get; set; }
        /// <summary>
        /// 传输协议
        /// </summary>
        public string ProtocolTypeString => ServerOption?.ProtocolType.ToString().ToUpper();
        /// <summary>
        /// 服务端Socket
        /// </summary>
        public Socket Socket { get; set; }
        /// <summary>
        /// 服务启动时间
        /// </summary>
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        /// <summary>
        /// 服务ID
        /// </summary>
        public string ServerId { get; set; } = Guid.NewGuid().ToString();
        /// <summary>
        /// 连接事件
        /// </summary>
        public Action<TSession> OnConnected { get; set; }
        /// <summary>
        /// 接收数据事件
        /// </summary>
        public Action<TSession, TRequestInfo> OnReceived { get; set; }
        /// <summary>
        /// 关闭事件
        /// </summary>
        public Action<TSession> OnClosed { get; set; }
        /// <summary>
        /// 启动服务
        /// </summary>
        /// <returns></returns>
        public virtual Task<bool> StartAysnc()
        {
            return Task.Run(() =>
            {
                bool isSuccess = false;
                try
                {
                    if (ServerOption == null) throw new ArgumentException("ServerOption服务配置尚未配置");
                    IPEndPoint iPEndPoint = new IPEndPoint(ServerOption.Ip == "Any" ? IPAddress.Any : IPAddress.Parse(ServerOption.Ip), ServerOption.Port);
                    if (ServerOption.ProtocolType == ProtocolType.Tcp)
                    {
                        #region TCP
                        Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                        {
                            NoDelay = ServerOption.NoDelay
                        };
                        Socket.Bind(iPEndPoint);
                        //开始监听
                        Socket.Listen(ServerOption.BackLog);
                        //开启一个线程接收客户端连接
                        Task.Run(async () =>
                        {
                            while (true)
                            {
                                var client = await Socket.AcceptAsync();
                                //构造客户端Session
                                var session = new TSession
                                {
                                    Socket = client,
                                    LocalEndPoint = client.LocalEndPoint,
                                    RemoteEndPoint = client.RemoteEndPoint
                                };
                                //非SSL
                                if (ServerOption.Security == SslProtocols.None)
                                {
                                    //Socket转字节流对象
                                    session.Stream = new NetworkStream(session.Socket, true);
                                    //字节流绑定到Pipe管道（高性能缓冲区）
                                    session.Reader = PipeReader.Create(session.Stream);//读取数据端
                                    session.Writer = PipeWriter.Create(session.Stream);//发送数据端

                                    //添加到Session容器
                                    SessionContainer.Add(session);
                                    OnConnected?.Invoke(session);
                                    //开启一个线程异步接收这个连接的数据
                                    ProcessReadAsync(session);
                                }
                                else//SSL
                                {
                                    //绑定SSL认证回调
                                    ServerOption.SslServerAuthenticationOptions.RemoteCertificateValidationCallback = SSLValidationCallback;
                                    //加密传输字节流
                                    var sslStream = new SslStream(new NetworkStream(session.Socket, true), false);
                                    var cancelTokenSource = new CancellationTokenSource();
                                    //SSL认证超时时间为5s
                                    cancelTokenSource.CancelAfter(5000);
                                    await sslStream.AuthenticateAsServerAsync(ServerOption.SslServerAuthenticationOptions, cancelTokenSource.Token).ContinueWith(t =>
                                    {
                                        if (sslStream.IsAuthenticated)
                                        {
                                            //验证成功
                                            session.Stream = sslStream;
                                            //字节流绑定到Pipe管道（高性能缓冲区）
                                            session.Reader = PipeReader.Create(session.Stream);//读取数据端
                                            session.Writer = PipeWriter.Create(session.Stream);//发送数据端

                                            //添加到Session容器
                                            SessionContainer.Add(session);
                                            OnConnected?.Invoke(session);
                                            //开启一个线程异步接收这个连接的数据
                                            ProcessReadAsync(session);
                                        }
                                        else
                                        {
                                            if (t.IsCanceled)
                                                LogHelper.Error($"连接{session.RemoteEndPoint}证书验证超时，关闭连接");
                                            Close(session);
                                        }
                                    });
                                }
                            }
                        });
                        #endregion
                    }
                    else if (ServerOption.ProtocolType == ProtocolType.Udp)
                    {
                        #region UDP
                        Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                        Socket.Bind(iPEndPoint);
                        EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                        var session = new TSession
                        {
                            Socket = Socket,
                            LocalEndPoint = Socket.LocalEndPoint
                        };
                        //异步接收这个连接的数据
                        Socket.BeginReceiveFrom(session.Data, 0, session.Data.Length, SocketFlags.None, ref remoteEP, UdpReceiveCallback, session);
                        #endregion
                    }
                    else
                        throw new Exception("不支持的传输协议");

                    isSuccess = true;
                }
                catch (Exception ex)
                {
                    LogHelper.Error($"【{ServerOption.Port}】端口启动监听异常：{ex}");
                }

                return isSuccess;
            });
        }
        /// <summary>
        /// 停止服务
        /// </summary>
        public virtual void Stop()
        {
            Socket.Close();
        }
        /// <summary>
        /// Session容器
        /// </summary>
        public SessionContainer<TSession> SessionContainer { get; set; } = new SessionContainer<TSession>();
        /// <summary>
        /// 下一个过滤器
        /// </summary>
        public IReceiveFilter<TRequestInfo> NextReceiveFilter { get; set; }
        /// <summary>
        /// 获取Session列表
        /// </summary>
        /// <param name="predicate">查询条件</param>
        public List<TSession> GetSessionList(Predicate<TSession> predicate = null)
        {
            return predicate == null ? SessionContainer.SessionList : SessionContainer.SessionList.FindAll(predicate);
        }
        /// <summary>
        /// 获取单个Session
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns></returns>
        public TSession GetSingleSession(Predicate<TSession> predicate)
        {
            return SessionContainer.SessionList.Find(predicate);
        }
        /// <summary>
        /// 连接数
        /// </summary>
        public long SessionCount => SessionContainer.SessionCount;

        #region 接收数据
        /// <summary>
        /// 异步接收数据
        /// </summary>
        /// <param name="session"></param>
        public void ProcessReadAsync(TSession session)
        {
            Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        //数据来了会得到一个ReadResult
                        ReadResult result = await session.Reader.ReadAsync();
                        ReadOnlySequence<byte> buffer = result.Buffer;

                        //开始位置和结束位置
                        SequencePosition consumed = buffer.Start;
                        SequencePosition examined = buffer.End;

                        try
                        {
                            //如果超时了直接退出
                            if (result.IsCanceled)
                            {
                                break;
                            }

                            var completed = result.IsCompleted;

                            if (buffer.Length > 0)
                            {
                                if (!ReaderBuffer(session, ref buffer, out consumed, out examined))
                                {
                                    completed = true;
                                    break;
                                }
                            }

                            if (completed)
                            {
                                break;
                            }
                        }
                        catch (Exception e)
                        {
                            LogHelper.Error($"接收数据出错，{e.Message}");
                            break;
                        }
                        finally
                        {
                            //标记消费数据的位置
                            session.Reader.AdvanceTo(consumed, examined);
                        }
                    }

                    //标记为完成
                    await session.Reader.CompleteAsync();
                }
                catch (Exception ex)
                {
                    LogHelper.Error($"接收数据出错，{ex.Message}");
                }
                finally
                {
                    //接收完毕或者出错后直接关闭连接
                    Close(session);
                }
            });
        }

        /// <summary>
        /// 读取并解析数据
        /// </summary>
        /// <param name="session"></param>
        /// <param name="buffer"></param>
        /// <param name="consumed"></param>
        /// <param name="examined"></param>
        /// <returns></returns>
        private bool ReaderBuffer(TSession session, ref ReadOnlySequence<byte> buffer, out SequencePosition consumed, out SequencePosition examined)
        {
            consumed = buffer.Start;
            examined = buffer.End;

            var bytesConsumedTotal = 0L;

            //最大请求（单位：字节）
            var maxPackageLength = ServerOption.MaxRequestLength;
            var seqReader = new SequenceReader<byte>(buffer);

            while (true)
            {
                mark:
                //过滤解析
                if (NextReceiveFilter != null)
                {
                    //有可能切换了过滤器，当前过滤器自定为下一个过滤器
                    if (NextReceiveFilter.NextReceiveFilter != null)
                        NextReceiveFilter = NextReceiveFilter.NextReceiveFilter;
                    //按用户指定的协议切割一个包
                    var packageInfo = NextReceiveFilter.Filter(ref seqReader);
                    var bytesConsumed = seqReader.Consumed;
                    bytesConsumedTotal += bytesConsumed;

                    var len = bytesConsumed;

                    //数据未消费，需要更多的数据
                    if (len == 0)
                        len = seqReader.Length;

                    if (maxPackageLength > 0 && len > maxPackageLength)
                    {
                        LogHelper.Error($"数据长度不能超过{maxPackageLength}个字节");
                        //直接关闭连接
                        Close(session);
                        return false;
                    }

                    //继续接收
                    if (packageInfo == null)
                    {
                        //如果下一个过滤器不为空，但是有没有收到完整的包，重走过滤器（一般是使用了多过滤器）
                        if (NextReceiveFilter.NextReceiveFilter != null && NextReceiveFilter.NextReceiveFilter != NextReceiveFilter)
                            goto mark;
                        consumed = buffer.GetPosition(bytesConsumedTotal);
                        return true;
                    }
                    if (!packageInfo.Success)
                    {
                        LogHelper.Error(packageInfo.Message);
                    }
                    else
                    {
                        OnReceived?.Invoke(session, packageInfo);
                    }

                    if (seqReader.End) //没有更多数据
                    {
                        examined = consumed = buffer.End;
                        return true;
                    }

                    seqReader = new SequenceReader<byte>(seqReader.Sequence.Slice(bytesConsumed));
                }
                else
                {
                    //没有过滤器来多少数据返回多少数据给用户
                    examined = consumed = buffer.End;
                    var packageInfo = new TRequestInfo
                    {
                        Success = true,
                        Raw = buffer.ToArray()
                    };
                    OnReceived?.Invoke(session, packageInfo);
                    return true;
                }
            }
        }

        /// <summary>
        /// 接收数据回调
        /// </summary>
        /// <param name="state"></param>
        async void UdpReceiveCallback(IAsyncResult asyncResult)
        {
            //服务端的Socket实例
            var session = (TSession)asyncResult.AsyncState;
            //定义远程的计算机地址
            EndPoint remouteEP = new IPEndPoint(IPAddress.Any, 0);
            //接收到的数据长度
            var length = session.Socket.EndReceiveFrom(asyncResult, ref remouteEP);
            //记录远程主机
            session.RemoteEndPoint = remouteEP;
            //查找客户端集合是否已存在该远程主机，存在的话直接用缓存的Session
            var querySession = GetSingleSession(c => c.RemoteEndPoint.ToString() == session.RemoteEndPoint.ToString());
            if (querySession != null)
            {
                querySession.RemoteEndPoint = remouteEP;
                session = querySession;
            }
            else
            {
                Pipe pipe = new Pipe();
                session.Reader = pipe.Reader;
                session.Writer = pipe.Writer;
                ProcessReadAsync(session);
                SessionContainer.Add(session);
                OnConnected?.Invoke(session);
            }
            //读取到的数据
            var data = session.Data.CloneRange(0, length);
            //将收到的数据写入Pipe管道
            await session.Writer.WriteAsync(new Memory<byte>(data));
            //继续异步接收数据
            var nextSession = new TSession { Socket = Socket, LocalEndPoint = Socket.LocalEndPoint };
            session.Socket.BeginReceiveFrom(nextSession.Data, 0, nextSession.Data.Length, SocketFlags.None, ref remouteEP, UdpReceiveCallback, nextSession);
        }
        #endregion

        /// <summary>
        /// 关闭一个连接
        /// </summary>
        /// <param name="session"></param>
        private void Close(TSession session)
        {
            try
            {
                SessionContainer.Remove(session);
                OnClosed?.Invoke(session);
                session.Close();
            }
            catch (Exception ex)
            {
                LogHelper.Error($"关闭连接出错，{ex.Message}");
            }
        }

        /// <summary>
        /// SSL回调认证，这里免去认证，直接返回true
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns></returns>
        private bool SSLValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
