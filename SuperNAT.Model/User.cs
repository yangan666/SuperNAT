using Dapper.Contrib.Extensions;
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
        public int login_times { get; set; }
        public DateTime? last_login_time { get; set; }
        [Write(false)]
        public string is_disabled_str => is_disabled ? "禁用" : "正常";
        [Write(false)]
        public string token { get; set; }
        [Write(false)]
        public List<Menu> menu_list { get; set; }
        [Write(false)]
        public string role_name { get; set; }
    }
}
