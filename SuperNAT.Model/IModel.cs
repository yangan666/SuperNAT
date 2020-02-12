using Dapper;
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
        [Editable(false)]
        public int page_index { get; set; }
        [Editable(false)]
        public int page_size { get; set; }
        [Editable(false)]
        public string search { get; set; }
    }
}
