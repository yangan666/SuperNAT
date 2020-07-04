using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Model
{
    [Table("role")]
    public class Role : BaseModel, IModel
    {
        [Key]
        public int id { get; set; }
        public string role_id { get; set; }
        public string name { get; set; }
        public string remark { get; set; }
        [Write(false)]
        public List<string> menu_ids { get; set; }
        [Write(false)]
        public string user_id { get; set; }
        [Write(false)]
        public bool is_admin { get; set; } = false;
    }
}
