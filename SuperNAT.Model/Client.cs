using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Model
{
    [Table("client")]
    public class Client : BaseModel, IModel
    {
        [Key]
        public int id { get; set; }
        public string name { get; set; }
        public string subdomain { get; set; }
        public string secret { get; set; }
        public bool is_online { get; set; }
        public DateTime? last_heart_time { get; set; }
        public string machine_id { get; set; }
        public string remark { get; set; }
        public string user_id { get; set; }
        public DateTime create_time { get; set; }
        [Write(false)]
        public string user_name { get; set; }
        [Write(false)]
        public string is_online_str => is_online ? "在线" : "离线";
        [Write(false)]
        public bool is_admin { get; set; } = false;
        [Write(false)]
        public List<Map> MapList { get; set; }
    }
}
