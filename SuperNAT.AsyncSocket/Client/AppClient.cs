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
    public abstract class AppClient<TSession, TRequestInfo> : ISession
        where TSession : ISession, new()
        where TRequestInfo : IRequestInfo, new()
    {
        public AppClient()
        {

        }
        public AppClient(IReceiveFilter<TRequestInfo> receiveFilter)
        {
            NextReceiveFilter = receiveFilter;
        }
        public void InitOption(ClientOption clientOption)
        {
            ClientOption = clientOption;
        }
        public ClientOption ClientOption { get; private set; }
        public Socket Socket { get; set; }
        public bool IsConnected { get; set; }
        public Stream Stream { get; set; }
        public PipeReader Reader { get; set; }
        public PipeWriter Writer { get; set; }
        public EndPoint LocalEndPoint { get; set; }
        public EndPoint RemoteEndPoint { get; set; }
        public string SessionId { get; set; } = Guid.NewGuid().ToString();
        public DateTime ConnectTime { get; set; } = DateTime.Now;
        public byte[] Data { get; set; } = new byte[2 * 1024 * 1024];//2M缓冲区
        public Action<object> OnConnected { get; set; }
        public Action<object, TRequestInfo> OnReceived { get; set; }
        public Action<object> OnClosed { get; set; }

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

        public async Task<bool> ConnectAsync()
        {
            try
            {
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    NoDelay = ClientOption.NoDelay
                };
                await Socket.ConnectAsync(new IPEndPoint(IPAddress.Parse(ClientOption.Ip), ClientOption.Port));
                RemoteEndPoint = Socket.RemoteEndPoint;
                LocalEndPoint = Socket.LocalEndPoint;
                if (ClientOption.Security == SslProtocols.None)
                {
                    Stream = new NetworkStream(Socket, true);
                    Reader = PipeReader.Create(Stream);
                    Writer = PipeWriter.Create(Stream);

                    IsConnected = true;
                    HandleLog.Log($"连接服务器[{ClientOption.Ip}:{ClientOption.Port}]成功");
                    OnConnected?.Invoke(this);
                    ProcessReadAsync();

                    return true;
                }
                else
                {
                    ClientOption.SslClientAuthenticationOptions.RemoteCertificateValidationCallback = SSLValidationCallback;
                    var sslStream = new SslStream(new NetworkStream(Socket, true), false);
                    var cancelTokenSource = new CancellationTokenSource();
                    cancelTokenSource.CancelAfter(5000);
                    return await sslStream.AuthenticateAsClientAsync(ClientOption.SslClientAuthenticationOptions, cancelTokenSource.Token).ContinueWith(t =>
                    {
                        if (sslStream.IsAuthenticated)
                        {
                            Stream = sslStream;
                            Reader = PipeReader.Create(Stream);
                            Writer = PipeWriter.Create(Stream);

                            IsConnected = true;
                            HandleLog.Log($"连接服务器[{ClientOption.Ip}:{ClientOption.Port}]成功");
                            OnConnected?.Invoke(this);
                            ProcessReadAsync();

                            return true;
                        }
                        else
                        {
                            if (t.IsCanceled)
                                HandleLog.Log($"连接{RemoteEndPoint}证书验证超时，关闭连接");
                            Close();
                            return false;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                if (IsConnected)
                {
                    Close();
                }
                IsConnected = false;
                HandleLog.Log($"客户端异常：{ex}");

                return false;
            }
        }

        public void Send(byte[] data)
        {
            try
            {
                if (ClientOption.ProtocolType == ProtocolType.Tcp)
                {
                    lock (Stream)
                        Stream.Write(data);
                }
                else
                {
                    EndPoint endPoint = new IPEndPoint(IPAddress.Parse(ClientOption.Ip), ClientOption.Port);
                    if (Socket == null)
                        Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    lock (Socket)
                        Socket.SendTo(data, endPoint);

                    EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                    var session = new TSession
                    {
                        Socket = Socket,
                        LocalEndPoint = Socket.LocalEndPoint
                    };
                    Socket.BeginReceiveFrom(session.Data, 0, session.Data.Length, SocketFlags.None, ref remoteEP, UdpReceiveCallback, session);
                }
            }
            catch (Exception ex)
            {
                Close();
                HandleLog.Log($"发送数据出错,{ex}");
            }
        }

        public void Close()
        {
            try
            {
                if (!IsConnected)
                    return;
                IsConnected = false;
                SessionContainer.Remove(SessionId);
                OnClosed?.Invoke(this);
                Socket?.Close();
            }
            catch (Exception ex)
            {
                HandleLog.Log($"关闭连接出错,{ex}");
            }
        }

        #region 接收数据
        /// <summary>
        /// 异步接收数据
        /// </summary>
        /// <param name="session"></param>
        public void ProcessReadAsync()
        {
            Task.Run(async () =>
            {
                try
                {
                    while (IsConnected)
                    {
                        ReadResult result = await Reader.ReadAsync();
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
                                if (!ReaderBuffer(ref buffer, out consumed, out examined))
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
                            HandleLog.Log($"接收数据出错，{e.Message}");
                            break;
                        }
                        finally
                        {
                            Reader.AdvanceTo(consumed, examined);
                        }
                    }

                    //标记为完成
                    await Reader.CompleteAsync();
                }
                catch (Exception ex)
                {
                    HandleLog.Log($"接收数据出错，{ex.Message}");
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
        private bool ReaderBuffer(ref ReadOnlySequence<byte> buffer, out SequencePosition consumed, out SequencePosition examined)
        {
            consumed = buffer.Start;
            examined = buffer.End;

            var bytesConsumedTotal = 0L;

            var maxPackageLength = ClientOption.MaxRequestLength;
            var seqReader = new SequenceReader<byte>(buffer);

            while (true)
            {
                mark:
                //过滤解析
                if (NextReceiveFilter != null)
                {
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
                        HandleLog.Log($"数据长度不能超过{maxPackageLength}个字节");
                        //直接关闭连接
                        Close();
                        return false;
                    }

                    //继续接收
                    if (packageInfo == null)
                    {
                        if (NextReceiveFilter.NextReceiveFilter != NextReceiveFilter)
                            goto mark;
                        consumed = buffer.GetPosition(bytesConsumedTotal);
                        return true;
                    }
                    if (!packageInfo.Success)
                    {
                        HandleLog.Log(packageInfo.Message);
                    }
                    else
                    {
                        OnReceived?.Invoke(this, packageInfo);
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
                    OnReceived?.Invoke(this, packageInfo);
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
                ProcessReadAsync();
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

        private bool SSLValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
