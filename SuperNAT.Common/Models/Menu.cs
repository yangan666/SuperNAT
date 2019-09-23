using Dapper;

namespace SuperNAT.Common.Models
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
        public bool hidden { get; set; }
        public bool away_show { get; set; }
        public string menu_id { get; set; }
        [Editable(false)]
        public string p_title { get; set; }
        [Editable(false)]
        public string hidden_str => hidden ? "√" : "";
        [Editable(false)]
        public string away_show_str => away_show ? "√" : "";
    }
}
