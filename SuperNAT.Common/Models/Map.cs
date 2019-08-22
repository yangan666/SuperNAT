using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Common.Models
{
    [Table("map")]
    public class Map
    {
        [Key]
        public int id { get; set; }
        public string name { get; set; } = "";
        public string local { get; set; } = "";
        public string remote { get; set; } = "";
        public string protocol { get; set; } = "http";
        public string certfile { get; set; } = "";
        public string certpwd { get; set; } = "";
        public int? ssl_type { get; set; }
        public int? client_id { get; set; }
        public bool is_disabled { get; set; }
        [Editable(false)]
        public string client_name { get; set; }
        [Editable(false)]
        public int? user_id { get; set; }
        [Editable(false)]
        public string user_name { get; set; }
    }
}
