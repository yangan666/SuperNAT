using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Common
{
    public class EnumHelper
    {
        public static List<KeyValue> EnumToList(Type type)
        {
            var rst = new List<KeyValue>();

            try
            {
                Array list = Enum.GetValues(type);
                foreach (var e in list)
                {
                    rst.Add(new KeyValue()
                    {
                        Key = Convert.ToInt32(e),
                        Value = e.ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
            }

            return rst;
        }
    }

    public class KeyValue
    {
        public int Key { get; set; }
        public string Value { get; set; }
    }

    public class StringKeyValue
    {
        public StringKeyValue(string key, string val)
        {
            Key = key;
            Value = val;
        }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
