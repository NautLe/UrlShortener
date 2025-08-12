namespace UrlShortener.Models
{
    public class ShortenResultViewModel
    {
        public string? OriginalUrl { get; set; }
        public string? ShortUrl { get; set; }
        public List<string>? Errors { get; set; }
    }
}
