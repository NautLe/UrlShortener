using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace UrlShortener.Services
{
    public class ShortCodeGenerator
    {
        private readonly Dictionary<string, string> _domainMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { "youtube.com", "yt" },
            { "facebook.com", "fb" },
            { "instagram.com", "ig" },
            { "twitter.com", "tw" },
            { "x.com", "x" },
            { "tiktok.com", "tt" },
            { "reddit.com", "rd" },
            { "linkedin.com", "in" },
            { "pinterest.com", "pt" },
            { "netflix.com", "nf" },
            { "amazon.com", "amz" },
            { "google.com", "gg" },
            { "discord.com", "dc" },
            { "twitch.tv", "twt" },
            { "spotify.com", "sp" },
            { "wikipedia.org", "wiki" },
            { "stackoverflow.com", "so" },
            { "github.com", "gh" },
            { "microsoft.com", "msft" },
            { "apple.com", "apl" },
        };

        public string Generate(string url)
        {
            Uri uri = new(url);
            string domain = uri.Host.Replace("www.", "");
            string path = uri.AbsolutePath.Trim('/');

            string domainShort = _domainMap.ContainsKey(domain)
                ? _domainMap[domain]
                : new string(domain.Take(3).ToArray());

            string[] pathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            string pathShort = string.Concat(pathParts
                .Select(part => Regex.Replace(part, "[^a-zA-Z0-9]", ""))
                .Where(clean => !string.IsNullOrEmpty(clean))
                .Select(clean => clean.Substring(0, Math.Min(3, clean.Length))));

            string finalCode = (domainShort + pathShort).ToLower();
            if (finalCode.Length > 10)
                finalCode = finalCode.Substring(0, 10);

            return finalCode;
        }
    }
}
