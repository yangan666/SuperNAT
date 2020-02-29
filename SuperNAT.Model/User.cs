using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Model
{
    [Table("user")]
    public class User : BaseModel, IModel
    {
        [Key]
        public int id { get; set; }
        public string user_id { get; set; }
        public string user_name { get; set; }
        public string password { get; set; }
        public string wechat { get; set; }
        public string email { get; set; }
        public string tel { get; set; }
        public bool is_disabled { get; set; } = false;
        public bool is_admin { get; set; } = false;
        public string role_id { get; set; }
        public DateTime create_time { get; set; } = DateTime.Now;
        [Editable(false)]
        public string is_disabled_str => is_disabled ? "禁用" : "正常";
        [Editable(false)]
        public string token { get; set; }
        [Editable(false)]
        public List<Menu> menu_list { get; set; }
        [Editable(false)]
        public string role_name { get; set; }
    }
}
