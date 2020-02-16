using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNAT.Common
{
    public static class JsonExtensions
    {
        public static string ToJson(this object obj)
        {
            return obj == null ? string.Empty : JsonHelper.Instance.Serialize(obj);
        }

        public static T FromJson<T>(this string str)
        {
            return string.IsNullOrWhiteSpace(str) ? default : JsonHelper.Instance.Deserialize<T>(str);
        }
    }
}
