using SuperNAT.Common;
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
    public class SocketServer<TSession, TRequestInfo> : IServer
        where TSession : ISession, new()
        where TRequestInfo : IRequestInfo, new()
    {
        Socket listenSocket;
        SessionManager<TSession> m_sessionManager = new SessionManager<TSession>();
        public string ServerId { get; set; } = Guid.NewGuid().ToString();
        public ServerOption ServerOption { get; set; }
        public IReceiveFilter<TRequestInfo> ReceiveFilter { get; set; }

        public long SessionCount => m_sessionManager.SessionCount;

        public Action<TSession> OnConnected;
        public Action<TSession, TRequestInfo> OnReceived;
        public Action<TSession> OnClosed;

        public SocketServer(ServerOption serverOption)
        {
            ServerOption = serverOption;
        }

        public virtual async Task StartAsync()
        {
            IPEndPoint localEndPoint = new IPEndPoint(ServerOption.Ip == "Any" ? IPAddress.Any : IPAddress.Parse(ServerOption.Ip), ServerOption.Port);
            listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ServerOption.ProtocolType);
            listenSocket.Bind(localEndPoint);
            listenSocket.Listen(ServerOption.BackLog);
            listenSocket.NoDelay = ServerOption.NoDelay;

            await Task.Run(async () =>
            {
                while (true)
                {
                    var socket = await listenSocket.AcceptAsync();

                    TSession session = new TSession()
                    {
                        Server = this,
                        Socket = socket,
                        Remote = socket.RemoteEndPoint.ToString(),
                        Local = socket.LocalEndPoint.ToString()
                    };
                    m_sessionManager.Add(session);
                    OnConnected?.Invoke(session);

                    // Create a PipeReader over the network stream
                    if (ServerOption.Security == SslProtocols.None)
                    {
                        session.Stream = new NetworkStream(session.Socket, true);

                        session.Reader = PipeReader.Create(session.Stream);
                        session.Writer = PipeWriter.Create(session.Stream);

                        _ = ProcessReadAsync(session);
                    }
                    else
                    {
                        ServerOption.SslServerAuthenticationOptions.RemoteCertificateValidationCallback = SSLValidationCallback;
                        var sslStream = new SslStream(new NetworkStream(session.Socket, true), false);
                        var cancelTokenSource = new CancellationTokenSource();
                        cancelTokenSource.CancelAfter(5000);
                        _ = sslStream.AuthenticateAsServerAsync(ServerOption.SslServerAuthenticationOptions, cancelTokenSource.Token).ContinueWith(t =>
                        {
                            if (sslStream.IsAuthenticated)
                            {
                                session.Stream = sslStream;

                                session.Reader = PipeReader.Create(session.Stream);
                                session.Writer = PipeWriter.Create(session.Stream);

                                _ = ProcessReadAsync(session);
                            }
                            else
                            {
                                if (t.IsCanceled)
                                    HandleLog.WriteLine($"连接{session.Remote}证书验证超时，关闭连接");
                                session.Close();
                            }
                        });
                    }
                }
            });
        }

        public void Stop()
        {
            listenSocket?.Close();
        }

        private async Task ProcessReadAsync(TSession session)
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
                        HandleLog.WriteLine($"ProcessReadAsync error,{e.Message}");
                        break;
                    }
                    finally
                    {
                        session.Reader.AdvanceTo(consumed, examined);
                    }
                }

                // Mark the PipeReader as complete.
                await session.Reader.CompleteAsync();
            }
            catch (Exception ex)
            {
                HandleLog.WriteLine($"ProcessReadAsync error,{ex.Message}");
            }
            finally
            {
                // close the connection if get a protocol error
                Close(session);
            }
        }

        private bool ReaderBuffer(TSession session, ref ReadOnlySequence<byte> buffer, out SequencePosition consumed, out SequencePosition examined)
        {
            consumed = buffer.Start;
            examined = buffer.End;

            var bytesConsumedTotal = 0L;

            var maxPackageLength = ServerOption.MaxRequestLength;
            var seqReader = new SequenceReader<byte>(buffer);

            while (true)
            {
                //过滤解析
                if (ReceiveFilter != null)
                {
                    var packageInfo = ReceiveFilter.Filter(ref seqReader);
                    var bytesConsumed = seqReader.Consumed;
                    bytesConsumedTotal += bytesConsumed;

                    var len = bytesConsumed;

                    // nothing has been consumed, need more data
                    if (len == 0)
                        len = seqReader.Length;

                    if (maxPackageLength > 0 && len > maxPackageLength)
                    {
                        HandleLog.WriteLine($"Package cannot be larger than {maxPackageLength}.");
                        // close the the connection directly
                        Close(session);
                        return false;
                    }

                    // continue receive...
                    if (packageInfo == null)
                    {
                        consumed = buffer.GetPosition(bytesConsumedTotal);
                        return true;
                    }
                    if (!packageInfo.Success)
                    {
                        HandleLog.WriteLine(packageInfo.Message);
                    }
                    else
                    {
                        OnReceived?.Invoke(session, packageInfo);
                    }

                    if (seqReader.End) // no more data
                    {
                        examined = consumed = buffer.End;
                        return true;
                    }

                    seqReader = new SequenceReader<byte>(seqReader.Sequence.Slice(bytesConsumed));
                }
                else
                {
                    examined = consumed = buffer.End;
                    var packageInfo = new TRequestInfo()
                    {
                        Success = true,
                        Raw = buffer.ToArray()
                    };
                    OnReceived?.Invoke(session, packageInfo);
                    return true;
                }
            }
        }

        private bool SSLValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public void Close(TSession session)
        {
            try
            {
                m_sessionManager.Remove(session);
                OnClosed?.Invoke(session);
                session.Socket?.Close();
            }
            catch (Exception ex)
            {
                HandleLog.WriteLine(ex.Message);
            }
        }

        public TSession GetSingle(Predicate<TSession> predicate)
        {
            return m_sessionManager.SessionList.Find(predicate);
        }

        public List<TSession> GetAll()
        {
            return m_sessionManager.SessionList;
        }

        public List<TSession> GetList(Predicate<TSession> predicate)
        {
            return m_sessionManager.SessionList.FindAll(predicate);
        }
    }
}
