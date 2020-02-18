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
    public class SocketClient<TRequestInfo> where TRequestInfo : IRequestInfo, new()
    {
        public Socket Socket { get; set; }
        public IReceiveFilter<TRequestInfo> ReceiveFilter { get; set; }
        public ClientOptions ClientOptions { get; set; }
        public bool IsConnected { get; set; } = false;
        public string Remote { get; set; }
        public string Local { get; set; }

        public Action<Socket> OnConnected;
        public Action<Socket, TRequestInfo> OnReceived;
        public Action<Socket> OnClosed;

        public SocketClient(ClientOptions clientOptions)
        {
            ClientOptions = clientOptions;
            Socket = new Socket(SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = clientOptions.NoDelay
            };
        }

        public void Initialize(IReceiveFilter<TRequestInfo> receiveFilter)
        {
            ReceiveFilter = receiveFilter;
        }

        public async Task ConnectAsync()
        {
            try
            {
                await Socket.ConnectAsync(new IPEndPoint(IPAddress.Parse(ClientOptions.Ip), ClientOptions.Port));
                IsConnected = true;
                Remote = Socket.RemoteEndPoint.ToString();
                Local = Socket.LocalEndPoint.ToString();
                HandleLog.WriteLine($"连接服务器[{ClientOptions.Ip}:{ClientOptions.Port}]成功");
                OnConnected?.Invoke(Socket);
                await ProcessReadAsync();
            }
            catch (Exception ex)
            {
                if (IsConnected)
                {
                    HandleLog.WriteLine($"{ex}");
                    Close();
                }
                IsConnected = false;
            }
        }

        private async Task ProcessReadAsync()
        {
            try
            {
                Stream stream = null;
                if (ClientOptions.Security == SslProtocols.None)
                {
                    stream = new NetworkStream(Socket);
                }
                else
                {
                    ClientOptions.SslClientAuthenticationOptions.RemoteCertificateValidationCallback = SSLValidationCallback;
                    var sslStream = new SslStream(new NetworkStream(Socket, true), false);
                    await sslStream.AuthenticateAsClientAsync(ClientOptions.SslClientAuthenticationOptions, CancellationToken.None);

                    stream = sslStream;
                }
                var reader = PipeReader.Create(stream);

                while (IsConnected)
                {
                    ReadResult result = await reader.ReadAsync();
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
                        HandleLog.WriteLine($"ProcessReadAsync error,{e.Message}");
                        break;
                    }
                    finally
                    {
                        reader.AdvanceTo(consumed, examined);
                    }
                }

                // Mark the PipeReader as complete.
                await reader.CompleteAsync();

                // close the connection if get a protocol error
                Close();
            }
            catch (Exception ex)
            {
                HandleLog.WriteLine($"ProcessReadAsync error,{ex.Message}");
            }
            finally
            {
                Close();
            }
        }

        private bool ReaderBuffer(ref ReadOnlySequence<byte> buffer, out SequencePosition consumed, out SequencePosition examined)
        {
            consumed = buffer.Start;
            examined = buffer.End;

            var bytesConsumedTotal = 0L;

            var maxPackageLength = ClientOptions.MaxRequestLength;
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
                        Close();
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
                        OnReceived?.Invoke(Socket, packageInfo);
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
                    var packageInfo = new TRequestInfo()
                    {
                        Success = true,
                        Raw = buffer.ToArray()
                    };
                    OnReceived?.Invoke(Socket, packageInfo);
                    return true;
                }
            }
        }

        private bool SSLValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public void Send(byte[] data)
        {
            try
            {
                Socket?.Send(data);
            }
            catch (Exception ex)
            {
                Close();
                HandleLog.WriteLine(ex.Message);
            }
        }

        public async Task SendAsync(ArraySegment<byte> buffer)
        {
            try
            {
                await Socket?.SendAsync(buffer, SocketFlags.None);
            }
            catch (Exception ex)
            {
                Close();
                HandleLog.WriteLine(ex.Message);
            }
        }

        public void Close()
        {
            try
            {
                IsConnected = false;
                OnClosed?.Invoke(Socket);
                Socket?.Close();
            }
            catch (Exception ex)
            {
                HandleLog.WriteLine(ex.Message);
            }
        }
    }
}
