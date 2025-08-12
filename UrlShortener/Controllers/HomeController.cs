using Microsoft.AspNetCore.Mvc;
using UrlShortener.Data;
using UrlShortener.Models;
using System;
using System.Linq;

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
            // Validate input
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

            // If already shortened → return existing
            var existing = _context.ShortUrls.FirstOrDefault(x => x.OriginalUrl == validatedUri.ToString());
            if (existing != null)
            {
                ViewBag.ShortUrl = $"{Request.Scheme}://{Request.Host}/{existing.ShortCode}";
                return View("Index");
            }

            // Create short code with "meaningful" style
            var hostPart = validatedUri.Host.Replace("www.", "").Split('.')[0];
            var shortHost = hostPart.Length > 3 ? hostPart.Substring(0, 3) : hostPart;

            var pathSegments = validatedUri.Segments
                .Select(s => s.Trim('/'))
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Length > 3 ? s.Substring(0, 3) : s)
                .ToList();

            var combined = shortHost + string.Join("", pathSegments);
            var shortCode = new string(combined.Where(char.IsLetterOrDigit).ToArray());

            // If duplicate short code, add a random number
            if (_context.ShortUrls.Any(x => x.ShortCode == shortCode))
                shortCode += new Random().Next(10, 99);

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
    }
}
