using Microsoft.AspNetCore.Mvc;
using UrlShortener.Data;
using UrlShortener.Models;
using UrlShortener.Services;
using System;
using System.Linq;

namespace UrlShortener.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ShortCodeGenerator _codeGenerator;

        public HomeController(ApplicationDbContext context, ShortCodeGenerator codeGenerator)
        {
            _context = context;
            _codeGenerator = codeGenerator;
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
                ViewBag.Error = "Invalid URL format.";
                return View("Index");
            }

            var existing = _context.ShortUrls.FirstOrDefault(x => x.OriginalUrl == validatedUri.ToString());
            if (existing != null)
            {
                ViewBag.ShortUrl = $"{Request.Scheme}://{Request.Host}/{existing.ShortCode}";
                return View("Index");
            }

            var shortCode = _codeGenerator.Generate(validatedUri.ToString());

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
    }
}
