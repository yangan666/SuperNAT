using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNAT.Common
{
    public class GlobalConfig
    {
        public static string ConnetionString { get; set; }
        public static int NatPort { get; set; }
        public static string WebPort { get; set; }
        public static List<int> WebPortList
        {
            get
            {
                var list = new List<int>();

                try
                {
                    if (!string.IsNullOrEmpty(WebPort))
                    {
                        var portArr = WebPort.Split(",");
                        foreach (var item in portArr)
                        {
                            if (item.Contains("-"))
                            {
                                var arr = item.Split("-");
                                int.TryParse(arr[0], out int start);
                                int.TryParse(arr[1], out int end);
                                if (start > 0 && end > 0)
                                {
                                    for (var i = start; i <= end; i++)
                                    {
                                        list.Add(i);
                                    }
                                }
                            }
                            else
                            {
                                if (int.TryParse(item, out int port))
                                {
                                    list.Add(port);
                                }
                            }
                        }
                    }
                }
                catch
                {

                }

                return list;
            }
        }
        public static string TcpPort { get; set; }
        public static List<int> TcpPortList
        {
            get
            {
                var list = new List<int>();

                try
                {
                    if (!string.IsNullOrEmpty(TcpPort))
                    {
                        var portArr = TcpPort.Split(",");
                        foreach (var item in portArr)
                        {
                            if (item.Contains("-"))
                            {
                                var arr = item.Split("-");
                                int.TryParse(arr[0], out int start);
                                int.TryParse(arr[1], out int end);
                                if (start > 0 && end > 0)
                                {
                                    for (var i = start; i <= end; i++)
                                    {
                                        list.Add(i);
                                    }
                                }
                            }
                            else
                            {
                                if (int.TryParse(item, out int port))
                                {
                                    list.Add(port);
                                }
                            }
                        }
                    }
                }
                catch
                {

                }

                return list;
            }
        }
        public static int ServerPort { get; set; }
        public static string DefaultUrl { get; set; }
        public static string RegRoleId { get; set; }
    }
}
