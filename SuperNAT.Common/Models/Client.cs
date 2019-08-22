using Dapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNAT.Common.Models
{
    [Table("client")]
    public class Client
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
        public int? user_id { get; set; }
        public DateTime create_time { get; set; }
        [Editable(false)]
        public string user_name { get; set; }
        [Editable(false)]
        public string is_online_str => is_online ? "在线" : "离线";
    }
}
