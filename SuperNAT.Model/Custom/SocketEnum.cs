using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNAT.Model
{
    public enum JsonType : byte
    {
        NAT = 0x01,
        HTTP = 0x02,
        WEBSOCKET = 0x03,
        TCP = 0x04,
        UDP = 0x05
    }

    public enum NatAction : byte
    {
        Connect = 0x01,
        Heart = 0x02,
        MapChange = 0x03,
        ServerMessage = 0x04
    }

    public enum TcpAction : byte
    {
        Connect = 0x01,
        TransferData = 0x02,
        Close = 0x03
    }

    public enum HttpAction : byte
    {
        Request = 0x01,
        Response = 0x02
    }

    public enum FilterStatus
    {
        None,
        LoadingHeader,
        LoadingBody,
        Completed
    }

    public enum ServerMessageType : byte
    {
        Info = 0,
        Error = 1
    }
}
