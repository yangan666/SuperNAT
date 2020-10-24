using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
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
    /// Socket客户端
    /// </summary>
    /// <typeparam name="TSession"></typeparam>
    /// <typeparam name="TRequestInfo"></typeparam>
    public abstract class AppClient<TSession, TRequestInfo> : ISession
        where TSession : ISession, new()
        where TRequestInfo : IRequestInfo, new()
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public AppClient()
        {

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="receiveFilter">过滤器</param>
        public AppClient(IReceiveFilter<TRequestInfo> receiveFilter)
        {
            NextReceiveFilter = receiveFilter;
        }

        /// <summary>
        /// 初始化客户端配置
        /// </summary>
        /// <param name="clientOption">客户端配置</param>
        public void InitOption(ClientOption clientOption)
        {
            ClientOption = clientOption;
        }

        /// <summary>
        /// 客户端配置
        /// </summary>
        public ClientOption ClientOption { get; private set; }

        /// <summary>
        /// 传输协议
        /// </summary>
        public string ProtocolTypeString => (ClientOption?.ProtocolType ?? Socket?.ProtocolType)?.ToString().ToUpper();
        /// <summary>
        /// 客户端Socket
        /// </summary>
        public Socket Socket { get; set; }
        /// <summary>
        /// 连接状态
        /// </summary>
        public bool IsConnected { get; set; }
        /// <summary>
        /// Socket字节流
        /// </summary>
        public Stream Stream { get; set; }
        /// <summary>
        /// Pipe管道读取缓冲区数据，数据来了直接读取，这是一个net core自带的高性能缓冲区，不用考虑数据大小，缓冲区大小会自适应
        /// </summary>
        public PipeReader Reader { get; set; }
        /// <summary>
        /// Pipe管道写入数据到缓冲区，可以充当Socket的Send方法
        /// </summary>
        public PipeWriter Writer { get; set; }
        /// <summary>
        /// 本机Socket节点
        /// </summary>
        public EndPoint LocalEndPoint { get; set; }
        /// <summary>
        /// 远端Socket节点
        /// </summary>
        public EndPoint RemoteEndPoint { get; set; }
        /// <summary>
        /// 会话ID
        /// </summary>
        public string SessionId { get; set; } = Guid.NewGuid().ToString();
        /// <summary>
        /// 连接时间
        /// </summary>
        public DateTime ConnectTime { get; set; } = DateTime.UtcNow;
        /// <summary>
        /// 缓冲区（UDP）
        /// </summary>
        public byte[] Data { get; set; } = new byte[2 * 1024 * 1024];//2M缓冲区
        /// <summary>
        /// 连接事件
        /// </summary>
        public Action<object> OnConnected { get; set; }
        /// <summary>
        /// 接收数据事件
        /// </summary>
        public Action<object, TRequestInfo> OnReceived { get; set; }
        /// <summary>
        /// 关闭事件
        /// </summary>
        public Action<object> OnClosed { get; set; }
        /// <summary>
        /// Session容器
        /// </summary>
        public SessionContainer<TSession> SessionContainer { get; set; } = new SessionContainer<TSession>();
        /// <summary>
        /// 下一个过滤器
        /// </summary>
        public IReceiveFilter<TRequestInfo> NextReceiveFilter { get; set; }
        /// <summary>
        /// 是否已经开始UDP接收
        /// </summary>
        public bool IsStartUdpReceive { get; set; }
        /// <summary>
        /// 获取Session列表（UDP广播的时候才用到）
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns></returns>
        public List<TSession> GetSessionList(Predicate<TSession> predicate = null)
        {
            return predicate == null ? SessionContainer.SessionList : SessionContainer.SessionList.FindAll(predicate);
        }
        /// <summary>
        /// 获取单个Session（UDP广播的时候才用到）
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns></returns>
        public TSession GetSingleSession(Predicate<TSession> predicate)
        {
            return SessionContainer.SessionList.Find(predicate);
        }
        /// <summary>
        /// 连接数（UDP广播的时候才用到）
        /// </summary>
        public long SessionCount { get; }
        /// <summary>
        /// 连接TCP服务
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ConnectAsync()
        {
            try
            {
                if (ClientOption == null) throw new ArgumentException("ClientOption客户端配置尚未配置");
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    NoDelay = ClientOption.NoDelay
                };
                //异步连接
                await Socket.ConnectAsync(new IPEndPoint(IPAddress.Parse(ClientOption.Ip), ClientOption.Port));
                RemoteEndPoint = Socket.RemoteEndPoint;
                LocalEndPoint = Socket.LocalEndPoint;
                //非SSL
                if (ClientOption.Security == SslProtocols.None)
                {
                    //Socket转字节流对象
                    Stream = new NetworkStream(Socket, true);
                    //字节流绑定到Pipe管道（高性能缓冲区）
                    Reader = PipeReader.Create(Stream);//读取数据端
                    Writer = PipeWriter.Create(Stream);//发送数据端

                    IsConnected = true;
                    LogHelper.Info($"连接服务器[{ClientOption.Ip}:{ClientOption.Port}]成功");
                    OnConnected?.Invoke(this);
                    //开始异步接收数据
                    ProcessReadAsync();

                    return true;
                }
                else
                {
                    //SSL回调
                    ClientOption.SslClientAuthenticationOptions.RemoteCertificateValidationCallback = SSLValidationCallback;
                    //加密传输字节流
                    var sslStream = new SslStream(new NetworkStream(Socket, true), false);
                    var cancelTokenSource = new CancellationTokenSource();
                    //SSL认证超时时间为5s
                    cancelTokenSource.CancelAfter(5000);
                    return await sslStream.AuthenticateAsClientAsync(ClientOption.SslClientAuthenticationOptions, cancelTokenSource.Token).ContinueWith(t =>
                    {
                        if (sslStream.IsAuthenticated)
                        {
                            //验证成功
                            Stream = sslStream;
                            //字节流绑定到Pipe管道（高性能缓冲区）
                            Reader = PipeReader.Create(Stream);//读取数据端
                            Writer = PipeWriter.Create(Stream);//发送数据端

                            IsConnected = true;
                            LogHelper.Info($"连接服务器[{ClientOption.Ip}:{ClientOption.Port}]成功");
                            OnConnected?.Invoke(this);
                            //开始异步接收数据
                            ProcessReadAsync();

                            return true;
                        }
                        else
                        {
                            //验证失败
                            if (t.IsCanceled)
                                LogHelper.Error($"连接{RemoteEndPoint}证书验证超时，关闭连接");
                            //关闭连接
                            Close();
                            return false;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                //处理异常直接关闭客户端
                if (IsConnected)
                {
                    Close();
                }
                IsConnected = false;
                LogHelper.Error($"客户端异常：{ex}");

                return false;
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data">数据</param>
        public void Send(byte[] data)
        {
            try
            {
                //发送对象（远程主机）
                EndPoint sendEndPoint = null;
                //TCP模式使用Stream，以便支持SSL，注意这里不能用PipeWriter（好像不支持SSL）
                if (ClientOption.ProtocolType == ProtocolType.Tcp)
                {
                    lock (Stream)
                        Stream.Write(data);
                    sendEndPoint = Socket.RemoteEndPoint;
                    //LogHelper.Info($"[{ProtocolTypeString}]向{Socket.RemoteEndPoint}发送数据【{data.ToHex()}】，共{data.Length}字节");
                }
                else//UDP
                {
                    if (Socket == null)
                    {
                        //初始化Socket
                        Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    }
                    lock (Socket)
                    {
                        sendEndPoint = new IPEndPoint(IPAddress.Parse(ClientOption.Ip), ClientOption.Port);
                        Socket.SendTo(data, sendEndPoint);
                        //开始异步接收，接收使用的递归，调用一次就好了，不用每次发送数据都调用
                        if (!IsStartUdpReceive)
                        {
                            EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                            var session = new TSession
                            {
                                Socket = Socket,
                                LocalEndPoint = Socket.LocalEndPoint
                            };
                            //异步接收数据
                            Socket.BeginReceiveFrom(session.Data, 0, session.Data.Length, SocketFlags.None, ref remoteEP, UdpReceiveCallback, session);

                            //标记为已经开始异步接收
                            IsStartUdpReceive = true;
                        }
                    }
                }
                //LogHelper.Info($"[{ProtocolTypeString}]向{sendEndPoint}发送数据【{data.ToHex()}】，共{data.Length}字节");
            }
            catch (Exception ex)
            {
                Close();
                LogHelper.Error($"发送数据出错,{ex}");
            }
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            try
            {
                if (!IsConnected)
                    return;
                IsConnected = false;
                //从容器移除Session
                SessionContainer.Remove(SessionId);
                //通知关闭事件
                OnClosed?.Invoke(this);
                Socket?.Close();
            }
            catch (Exception ex)
            {
                LogHelper.Error($"关闭连接出错,{ex}");
            }
        }

        #region 接收数据
        /// <summary>
        /// 异步接收数据
        /// </summary>
        /// <param name="session"></param>
        public void ProcessReadAsync(TSession session = default)
        {
            Task.Run(async () =>
            {
                try
                {
                    if (session != null)
                        Reader = session.Reader;
                    while (IsConnected || session.IsConnected)
                    {
                        //数据来了会得到一个ReadResult
                        ReadResult result = await Reader.ReadAsync();
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
                                if (!ReaderBuffer(ref buffer, out consumed, out examined, session))
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
                            Reader.AdvanceTo(consumed, examined);
                        }
                    }

                    //标记为完成
                    await Reader.CompleteAsync();
                }
                catch (Exception ex)
                {
                    LogHelper.Error($"接收数据出错，{ex.Message}");
                }
                finally
                {
                    //接收完毕或者出错后直接关闭连接
                    Close();
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
        private bool ReaderBuffer(ref ReadOnlySequence<byte> buffer, out SequencePosition consumed, out SequencePosition examined, TSession session = default)
        {
            consumed = buffer.Start;
            examined = buffer.End;

            var bytesConsumedTotal = 0L;

            //最大请求（单位：字节）
            var maxPackageLength = ClientOption.MaxRequestLength;
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
                        Close();
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
                        OnReceived?.Invoke(session == null ? this : (object)session, packageInfo);
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
                    OnReceived?.Invoke(session == null ? this : (object)session, packageInfo);
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
                session.IsConnected = true;
                ProcessReadAsync(session);
                //有可能是广播，添加到客户端集合
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
