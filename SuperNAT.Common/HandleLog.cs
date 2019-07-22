using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Common
{
    public class HandleLog
    {
        public static Action<string> WriteLog { get; set; }
        public static void WriteLine(string log)
        {
            WriteLog?.Invoke(log);
        }
    }
}
