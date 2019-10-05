using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Common.Models
{
    [Table("role")]
    public class Role : BaseModel, IModel
    {
        [Key]
        public int id { get; set; }
        public string role_id { get; set; }
        public string name { get; set; }
        public string remark { get; set; }
        [Editable(false)]
        public List<string> menu_ids { get; set; }
    }
}
