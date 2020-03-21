using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperNAT.Common
{
    public static class StringExtensions
    {
        /// <summary>
        /// 根据切割符截取左边第一个字符串，返回右边字符串
        /// </summary>
        /// <param name="strValue"></param>
        /// <param name="separator"></param>
        /// <param name="left">左边第一个字符串</param>
        /// <returns></returns>
        public static string SubLeftWith(this string strValue, char separator, out string left)
        {
            if (string.IsNullOrEmpty(strValue))
            {
                left = string.Empty;
                return string.Empty;
            }

            var position = strValue.IndexOf(separator);
            left = strValue.Substring(0, position);

            return strValue.Substring(position + 1, strValue.Length - left.Length - 1).Trim();//去掉前后空格
        }

        /// <summary>
        /// 根据If条件是否返回字符串
        /// </summary>
        /// <param name="str">传入的字符串</param>
        /// <param name="condition">条件</param>
        /// <param name="falseStr">条件为false</param>
        /// <returns></returns>
        public static string If(this string str, bool condition, string falseStr = "")
        {
            return condition ? str : falseStr;
        }

        public static string ToLikeString(this string str, string joinStr, string likeStr)
        {
            return string.Join(joinStr, str.Split(',').Select(s => $" {s} like @{likeStr} "));
        }

        public static bool EqualsWhithNoCase(this string str, string compare)
        {
            return str.ToUpper() == compare.ToUpper();
        }
    }
}
