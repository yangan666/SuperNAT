using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace SuperNAT.Common
{
    public static class NameValueCollectionExtensions
    {
        public static Dictionary<string, string> ToDictionary(this NameValueCollection nameValueCollection)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            foreach (var key in nameValueCollection.AllKeys)
            {
                dict.Add(key, nameValueCollection[key]);
            }

            return dict;
        }
    }
}
