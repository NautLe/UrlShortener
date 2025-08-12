using Microsoft.AspNetCore.Mvc;
using UrlShortener.Data;
using UrlShortener.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace UrlShortener.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UrlController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UrlController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("shorten")]
        public IActionResult Shorten([FromBody] string originalUrl)
        {
            if (string.IsNullOrWhiteSpace(originalUrl) || !Uri.IsWellFormedUriString(originalUrl, UriKind.Absolute))
                return BadRequest("Invalid URL format.");

            // Check if already exists
            var existing = _context.ShortUrls.FirstOrDefault(u => u.OriginalUrl == originalUrl);
            if (existing != null)
                return Ok($"{Request.Scheme}://{Request.Host}/{existing.ShortCode}");

            // Generate code
            var shortCode = GenerateMeaningfulShortCode(originalUrl);

            int counter = 1;
            while (_context.ShortUrls.Any(u => u.ShortCode == shortCode))
            {
                shortCode = shortCode + counter;
                if (shortCode.Length > 10)
                    shortCode = shortCode.Substring(0, 10);
                counter++;
            }

            var shortUrl = new ShortUrl
            {
                OriginalUrl = originalUrl,
                ShortCode = shortCode
            };

            _context.ShortUrls.Add(shortUrl);
            _context.SaveChanges();

            var result = $"{Request.Scheme}://{Request.Host}/{shortCode}";
            return Ok(result);
        }

        [HttpGet("{shortCode}")]
        public IActionResult GetOriginalUrl(string shortCode)
        {
            if (string.IsNullOrWhiteSpace(shortCode))
                return BadRequest("Short code is required.");

            var link = _context.ShortUrls.FirstOrDefault(u => u.ShortCode == shortCode);
            if (link == null)
                return NotFound("Short link not found.");

            return Ok(new
            {
                OriginalUrl = link.OriginalUrl,
                ShortCode = link.ShortCode,
                ShortUrl = $"{Request.Scheme}://{Request.Host}/{link.ShortCode}",
                ClickCount = link.ClickCount
            });
        }

        private string GenerateMeaningfulShortCode(string url)
        {
            Uri uri = new Uri(url);
            string domain = uri.Host.Replace("www.", "");
            string path = uri.AbsolutePath.Trim('/');

            Dictionary<string, string> domainMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "github.com", "gh" },
                { "facebook.com", "fb" },
                { "google.com", "gg" },
                { "linkedin.com", "in" },
                { "twitter.com", "tw" },
                { "youtube.com", "yt" }
            };

            string domainShort = domainMap.ContainsKey(domain)
                ? domainMap[domain]
                : new string(domain.Take(3).ToArray());

            string[] pathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            string pathShort = "";
            foreach (var part in pathParts)
            {
                string clean = Regex.Replace(part, "[^a-zA-Z0-9]", "");
                if (!string.IsNullOrEmpty(clean))
                {
                    pathShort += clean.Substring(0, Math.Min(3, clean.Length));
                }
            }

            string finalCode = (domainShort + pathShort).ToLower();

            if (finalCode.Length > 10)
                finalCode = finalCode.Substring(0, 10);

            return finalCode;
        }
    }
}
