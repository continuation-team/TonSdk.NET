using System;

namespace TonSdk.Connect
{
    public class UrlUtils
    {
        public static string RemoveUrlLastSlash(string url)
        {
            if (url.EndsWith("/"))
            {
                return url.Substring(0, url.Length - 1);
            }

            return url;
        }

        public static string AddPathToUrl(string url, string path)
        {
            return RemoveUrlLastSlash(url) + "/" + path;
        }

        public static bool IsTelegramUrl(string link)
        {
            if (string.IsNullOrEmpty(link))
            {
                return false;
            }

            try
            {
                var url = new Uri(link);
                return url.Scheme == "tg" || url.Host == "t.me";
            }
            catch (UriFormatException)
            {
                return false;
            }
        }

        public static string EncodeTelegramUrlParameters(string parameters)
        {
            return parameters
                .Replace(".", "%2E")
                .Replace("-", "%2D")
                .Replace("_", "%5F")
                .Replace("&", "-")
                .Replace("=", "__")
                .Replace("%", "--");
        }
    }
}