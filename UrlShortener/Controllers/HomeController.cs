using Microsoft.AspNetCore.Mvc;
using UrlShortener.Data;
using UrlShortener.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace UrlShortener.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index() => View();

        [HttpPost]
        public IActionResult Shorten(string originalUrl)
        {
            if (string.IsNullOrWhiteSpace(originalUrl))
            {
                ViewBag.Error = "URL cannot be empty.";
                return View("Index");
            }

            if (!Uri.TryCreate(originalUrl, UriKind.Absolute, out Uri? validatedUri) ||
                (validatedUri.Scheme != Uri.UriSchemeHttp && validatedUri.Scheme != Uri.UriSchemeHttps))
            {
                ViewBag.Error = "Invalid URL format. Please enter a valid HTTP or HTTPS URL.";
                return View("Index");
            }

            // Check if already shortened
            var existing = _context.ShortUrls.FirstOrDefault(x => x.OriginalUrl == validatedUri.ToString());
            if (existing != null)
            {
                ViewBag.ShortUrl = $"{Request.Scheme}://{Request.Host}/{existing.ShortCode}";
                return View("Index");
            }

            // Generate meaningful short code
            var shortCode = GenerateMeaningfulShortCode(validatedUri.ToString());

            // Avoid duplicates
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
                OriginalUrl = validatedUri.ToString(),
                ShortCode = shortCode,
                CreatedAt = DateTime.UtcNow
            };

            _context.ShortUrls.Add(shortUrl);
            _context.SaveChanges();

            ViewBag.ShortUrl = $"{Request.Scheme}://{Request.Host}/{shortCode}";
            return View("Index");
        }

        private string GenerateMeaningfulShortCode(string url)
        {
            Uri uri = new Uri(url);
            string domain = uri.Host.Replace("www.", "");
            string path = uri.AbsolutePath.Trim('/');

            // Mapping domain
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

            // Path short
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
