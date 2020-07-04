using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNAT.Model
{
    public interface IModel
    {
        int id { get; set; }
        string user_id { get; set; }
        bool is_admin { get; set; }
    }

    public class BaseModel
    {
        [Write(false)]
        public int page_index { get; set; }
        [Write(false)]
        public int page_size { get; set; }
        [Write(false)]
        public string search { get; set; }
    }
}
