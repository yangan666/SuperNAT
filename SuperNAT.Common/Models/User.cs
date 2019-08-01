using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Common.Models
{
    [Table("user")]
    public class User
    {
        [Key]
        public int id { get; set; }
        public string user_name { get; set; }
        public string password { get; set; }
        public string wechat { get; set; }
        public string tel { get; set; }
        public string is_disabled { get; set; }
    }
}
