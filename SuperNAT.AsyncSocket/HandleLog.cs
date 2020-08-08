using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.AsyncSocket
{
    public class HandleLog
    {
        public static Action<string, bool> WriteLog { get; set; }
        public static void Log(string log, bool isPrint = true)
        {
            WriteLog?.Invoke(log, isPrint);
        }
    }
}
