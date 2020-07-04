using Dapper.Contrib.Extensions;

namespace SuperNAT.Model
{
    [Table("menu")]
    public class Menu : BaseModel, IModel
    {
        [Key]
        public int id { get; set; }
        public string pid { get; set; }
        public string name { get; set; }
        public string path { get; set; }
        public string component { get; set; }
        public string title { get; set; }
        public string icon { get; set; }
        public int sort_no { get; set; }
        public bool hidden { get; set; }
        public bool always_show { get; set; }
        public string menu_id { get; set; }
        [Write(false)]
        public string p_title { get; set; }
        [Write(false)]
        public string hidden_str => hidden ? "√" : "";
        [Write(false)]
        public string always_show_str => always_show ? "√" : "";
        [Write(false)]
        public string user_id { get; set; }
        [Write(false)]
        public bool is_admin { get; set; } = false;
    }
}
