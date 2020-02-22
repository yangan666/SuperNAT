using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNAT.Common
{
    public static class GlobalConfig
    {
        public static string ConnetionString { get; set; }
        public static int NatPort { get; set; }
        public static int ServerPort { get; set; }
        public static string DefaultUrl { get; set; }
        public static string RegRoleId { get; set; }
        public static string CertFile = "iot3rd.p12";
        public static string CertPassword = "IoM@1234";
    }
}
