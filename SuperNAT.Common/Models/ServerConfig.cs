using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Common.Models
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
        [Editable(false)]
        public string user_id { get; set; }
        [Editable(false)]
        public bool is_admin { get; set; }
        public string is_disabled_str => is_disabled ? "禁用" : "启用";
    }
}
