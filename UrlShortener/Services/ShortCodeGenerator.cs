namespace UrlShortener.Services
{
    public class ShortCodeGenerator
    {
        private readonly Dictionary<string, string> _domainMap = new()
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
            { "cms.greenwich.edu.vn", "cms" },
            { "ap.greenwich.edu.vn", "ap" },
        };

        public string Generate(string originalUrl)
        {
            if (Uri.TryCreate(originalUrl, UriKind.Absolute, out var uri))
            {
                var host = uri.Host.Replace("www.", "");
                if (_domainMap.TryGetValue(host, out var codePrefix))
                {
                    return $"{codePrefix}{Guid.NewGuid().ToString("N")[..4]}";
                }
            }

            return Guid.NewGuid().ToString("N")[..6];
        }
    }
}
