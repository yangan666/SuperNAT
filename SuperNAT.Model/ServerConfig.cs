using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Model
{
    [Table("server_config")]
    public class ServerConfig : BaseModel, IModel
    {
        [Key]
        public int id { get; set; }
        public string protocol { get; set; }
        public string port { get; set; }
        public bool is_ssl { get; set; } = false;
        public int? ssl_type { get; set; }
        public string certfile { get; set; }
        public string certpwd { get; set; }
        public bool is_disabled { get; set; } = false;
        [Write(false)]
        public string user_id { get; set; }
        [Write(false)]
        public bool is_admin { get; set; }
        [Write(false)]
        public string is_disabled_str => is_disabled ? "禁用" : "启用";
        [Write(false)]
        public List<int> port_list
        {
            get
            {
                var list = new List<int>();

                try
                {
                    if (!string.IsNullOrEmpty(port))
                    {
                        var portArr = port.Split(",");
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
    }
}
