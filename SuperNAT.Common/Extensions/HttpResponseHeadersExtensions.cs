using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace SuperNAT.Common
{
    public static class HttpResponseHeadersExtensions
    {
        public static Dictionary<string, string> ToDictionary(this HttpResponseHeaders httpResponseHeaders)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            foreach (var item in httpResponseHeaders)
            {
                dict.Add(item.Key, string.Join(';', item.Value));
            }

            return dict;
        }
    }
}
