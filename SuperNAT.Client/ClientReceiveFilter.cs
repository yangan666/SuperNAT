using SuperSocket.ProtoBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Client
{
    public class ClientReceiveFilter : IReceiveFilter<ClientPackageInfo>
    {
        public ClientReceiveFilter()
        {
            
        }

        public IReceiveFilter<ClientPackageInfo> NextReceiveFilter { get; set; }

        public FilterState State { get; set; }

        public ClientPackageInfo Filter(BufferList data, out int rest)
        {
            rest = 0;
            List<byte> bytes = new List<byte>();
            foreach (var item in data)
            {
                bytes.AddRange(item.ToList());
            }
            return new ClientPackageInfo(bytes.ToArray());
        }

        public void Reset()
        {
            
        }
    }
}
