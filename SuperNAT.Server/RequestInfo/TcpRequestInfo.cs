using SuperNAT.AsyncSocket;

namespace SuperNAT.Server
{
    public class TcpRequestInfo : RequestInfo
    {
        public TcpRequestInfo(byte[] data)
        {
            Raw = data;
        }
    }
}
