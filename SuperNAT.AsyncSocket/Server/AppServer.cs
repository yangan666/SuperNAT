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
    public abstract class AppServer<TSession, TRequestInfo> : IServer<TSession, TRequestInfo>
        where TSession : ISession, new()
        where TRequestInfo : IRequestInfo, new()
    {
        public AppServer(IReceiveFilter<TRequestInfo> receiveFilter)
        {
            NextReceiveFilter = receiveFilter;
        }
        public void InitOption(ServerOption serverOption)
        {
            ServerOption = serverOption;
        }
        public ServerOption ServerOption { get; set; }
        public Socket Socket { get; set; }
        public DateTime StartTime { get; set; } = DateTime.Now;
        public string ServerId { get; set; } = Guid.NewGuid().ToString();
        public Action<TSession> OnConnected { get; set; }
        public Action<TSession, TRequestInfo> OnReceived { get; set; }
        public Action<TSession> OnClosed { get; set; }
        public virtual Task<bool> StartAysnc()
        {
            return Task.Run(() =>
            {
                bool isSuccess = false;
                try
                {
                    IPEndPoint iPEndPoint = new IPEndPoint(ServerOption.Ip == "Any" ? IPAddress.Any : IPAddress.Parse(ServerOption.Ip), ServerOption.Port);
                    if (ServerOption.ProtocolType == ProtocolType.Tcp)
                    {
                        #region TCP
                        if (ServerOption == null) throw new ArgumentException("ServerOption服务配置尚未配置");
                        Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                        {
                            NoDelay = ServerOption.NoDelay
                        };
                        Socket.Bind(iPEndPoint);
                        Socket.Listen(ServerOption.BackLog);
                        Task.Run(async () =>
                        {
                            while (true)
                            {
                                var client = await Socket.AcceptAsync();
                                var session = new TSession
                                {
                                    Socket = client,
                                    LocalEndPoint = client.LocalEndPoint,
                                    RemoteEndPoint = client.RemoteEndPoint
                                };
                                if (ServerOption.Security == SslProtocols.None)
                                {
                                    session.Stream = new NetworkStream(session.Socket, true);
                                    session.Reader = PipeReader.Create(session.Stream);
                                    session.Writer = PipeWriter.Create(session.Stream);

                                    SessionContainer.Add(session);
                                    OnConnected?.Invoke(session);
                                    ProcessReadAsync(session);
                                }
                                else
                                {
                                    ServerOption.SslServerAuthenticationOptions.RemoteCertificateValidationCallback = SSLValidationCallback;
                                    var sslStream = new SslStream(new NetworkStream(session.Socket, true), false);
                                    var cancelTokenSource = new CancellationTokenSource();
                                    cancelTokenSource.CancelAfter(5000);
                                    await sslStream.AuthenticateAsServerAsync(ServerOption.SslServerAuthenticationOptions, cancelTokenSource.Token).ContinueWith(t =>
                                    {
                                        if (sslStream.IsAuthenticated)
                                        {
                                            session.Stream = sslStream;
                                            session.Reader = PipeReader.Create(session.Stream);
                                            session.Writer = PipeWriter.Create(session.Stream);

                                            SessionContainer.Add(session);
                                            OnConnected?.Invoke(session);
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
        public virtual void Stop()
        {
            Socket.Close();
        }
        public SessionContainer<TSession> SessionContainer { get; set; } = new SessionContainer<TSession>();
        public IReceiveFilter<TRequestInfo> NextReceiveFilter { get; set; }
        public List<TSession> GetSessionList(Predicate<TSession> predicate = null)
        {
            return predicate == null ? SessionContainer.SessionList : SessionContainer.SessionList.FindAll(predicate);
        }
        public TSession GetSingleSession(Predicate<TSession> predicate)
        {
            return SessionContainer.SessionList.Find(predicate);
        }
        public long SessionCount { get; }

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
                        ReadResult result = await session.Reader.ReadAsync();
                        ReadOnlySequence<byte> buffer = result.Buffer;

                        SequencePosition consumed = buffer.Start;
                        SequencePosition examined = buffer.End;

                        try
                        {
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

            var maxPackageLength = ServerOption.MaxRequestLength;
            var seqReader = new SequenceReader<byte>(buffer);

            while (true)
            {
                mark:
                //过滤解析
                if (NextReceiveFilter != null)
                {
                    //如果修改了下一个过滤器则使用下一个过滤器
                    if (NextReceiveFilter.NextReceiveFilter != null)
                        NextReceiveFilter = NextReceiveFilter.NextReceiveFilter;

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
                        //如果修下一个过滤器不为BULL切设置了，说明使用了多过滤器，packageInfo返回null表示需要重走过滤器
                        if (NextReceiveFilter.NextReceiveFilter != null && NextReceiveFilter.NextReceiveFilter != NextReceiveFilter)
                            goto mark;
                        consumed = buffer.GetPosition(bytesConsumedTotal);
                        return true;
                    }
                    if (!packageInfo.Success)
                    {
                        LogHelper.Info(packageInfo.Message);
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

        private bool SSLValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
