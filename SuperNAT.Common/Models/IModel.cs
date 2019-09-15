using Dapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNAT.Common.Models
{
    public interface IModel
    {
        int id { get; set; }
    }

    public class BaseModel
    {
        [Editable(false)]
        public int page_index { get; set; }
        [Editable(false)]
        public int page_size { get; set; }
    }
}
