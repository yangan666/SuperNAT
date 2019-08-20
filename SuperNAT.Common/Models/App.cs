using Dapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNAT.Common.Models
{
    [Table("app")]
    public class App
    {
        public int id { get; set; }
        public string name { get; set; }
        public string secret { get; set; }
        public string remark { get; set; }
        public int user_id { get; set; }
    }
}
