using SuperNAT.AsyncSocket;
using SuperNAT.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNAT.Core
{
    public class NatSession : AppSession<NatSession, NatRequestInfo>
    {
        public Client Client { get; set; }
        public List<Map> MapList { get; set; } = new List<Map>();
    }
}
