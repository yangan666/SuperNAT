using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Model
{
    [Table("request")]
    public class Request : BaseModel, IModel
    {
        [Key]
        public int id { get; set; }
        public string request_url { get; set; }
        public string client_ip { get; set; }
        public string user_id { get; set; }
        public DateTime? request_time { get; set; }
        public DateTime? response_time { get; set; }
        public DateTime? handle_time { get; set; }
        public DateTime? create_time { get; set; }
        public string requet_content { get; set; }
        public string response_content { get; set; }
        public int? map_id { get; set; }
        [Write(false)]
        public bool is_admin { get; set; } = false;
    }
}
