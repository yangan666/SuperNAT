using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Common.Models
{
    [Table("map")]
    public class Map : BaseModel, IModel
    {
        [Key]
        public int id { get; set; }
        public string name { get; set; }
        public string local { get; set; } = "localhost:80";
        public string remote { get; set; } = GlobalConfig.DefaultUrl;
        public string protocol { get; set; } = "http";
        public string certfile { get; set; }
        public string certpwd { get; set; }
        public int? ssl_type { get; set; }
        public int? client_id { get; set; }
        public bool is_disabled { get; set; }
        [Editable(false)]
        public string client_name { get; set; }
        [Editable(false)]
        public bool is_online { get; set; }
        [Editable(false)]
        public string is_online_str => is_online ? "在线" : "离线";
        [Editable(false)]
        public string user_id { get; set; }
        [Editable(false)]
        public string user_name { get; set; }
        [Editable(false)]
        public bool is_admin { get; set; } = false;
        [Editable(false)]
        public int ChangeType { get; set; }
    }
}
