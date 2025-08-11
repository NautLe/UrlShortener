using Microsoft.AspNetCore.Mvc;
using UrlShortener.Data;
using UrlShortener.Models;
using System;
using System.Text.RegularExpressions;

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
            // Requirement #3: Validate input URL properly
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

            // Prevent duplicate entries — if already shortened, return existing short URL
            var existing = _context.ShortUrls.FirstOrDefault(x => x.OriginalUrl == validatedUri.ToString());
            if (existing != null)
            {
                ViewBag.ShortUrl = $"{Request.Scheme}://{Request.Host}/{existing.ShortCode}";
                return View("Index");
            }

            // Generate short code
            var shortCode = Guid.NewGuid().ToString("N").Substring(0, 6);

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
