using System.Collections.Generic;
using System.Linq;
using System;

namespace Games.Coresdk.Unity
{
    public class URLUtility
    {
        const string DEEP_LINK_SCHEME = "coresdk";
        const string DEEP_LINK_HOST = "coresdk.games";

        public static string GetDeepLinkURL(string packageName = null, string lastPath = null)
        {
            if (string.IsNullOrEmpty(lastPath))
            {
                return string.Format("{0}://{1}/{2}", DEEP_LINK_SCHEME, DEEP_LINK_HOST, packageName);
            }

            return string.Format("{0}://{1}/{2}/{3}", DEEP_LINK_SCHEME, DEEP_LINK_HOST, packageName, lastPath);
        }

        public static string GetAuthorizationToken(string url)
        {
            var query = TokenCollection.Parse(url);
            return query.GetValue("token");
        }
    }

    public class TokenCollection
    {
        public static TokenCollection Parse(string url)
        {
            if (string.IsNullOrEmpty(url))
                return new TokenCollection(new Dictionary<string, string>());

            return Parse(new Uri(url));
        }

        static TokenCollection Parse(Uri uri)
        {
            var query = uri.Query.Substring(uri.Query.IndexOf('?') + 1);//+1 for skipping '?'
            var pairs = query.Split('&');
            var dict = pairs
               .Select(o => o.Split('='))
               .Where(items => items.Count() == 2)
               .ToDictionary(pair => Uri.UnescapeDataString(pair[0]),
                pair => Uri.UnescapeDataString(pair[1]));

            return new TokenCollection(dict);
        }

        private Dictionary<string, string> dict;

        TokenCollection(Dictionary<string, string> dict)
        {
            this.dict = dict;
        }

        public string GetValue(string key, string defaultValue = "")
        {
            string token = null;
            if (dict.TryGetValue(key, out token))
                return token;

            return defaultValue;
        }
    }
}