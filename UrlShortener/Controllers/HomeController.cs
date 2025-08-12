using Microsoft.AspNetCore.Mvc;
using UrlShortener.Data;
using UrlShortener.Models;
using UrlShortener.Services;

namespace UrlShortener.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ShortCodeGenerator _generator;

        public HomeController(ApplicationDbContext context, ShortCodeGenerator generator)
        {
            _context = context;
            _generator = generator;
        }

        public IActionResult Index()
        {
            return View(new ShortenRequest()); 
        }

        [HttpPost]
        public IActionResult Shorten(ShortenRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", request);
            }

            var validatedUrl = request.OriginalUrl!.Trim();

            // Check duplicate
            var existing = _context.ShortUrls.FirstOrDefault(x => x.OriginalUrl == validatedUrl);
            if (existing != null)
            {
                ViewBag.ShortUrl = $"{Request.Scheme}://{Request.Host}/{existing.ShortCode}";
                return View("Index", request);
            }

            // Generate unique shortcode
            string finalCode = _generator.Generate(validatedUrl);
            int counter = 1;
            while (_context.ShortUrls.Any(u => u.ShortCode == finalCode))
            {
                finalCode = (finalCode.Length >= 45)
                    ? finalCode.Substring(0, 45)
                    : finalCode + counter;
                counter++;
            }

            var shortUrl = new ShortUrl
            {
                OriginalUrl = validatedUrl,
                ShortCode = finalCode,
                CreatedAt = DateTime.UtcNow
            };

            _context.ShortUrls.Add(shortUrl);
            _context.SaveChanges();

            ViewBag.ShortUrl = $"{Request.Scheme}://{Request.Host}/{finalCode}";
            return View("Index", request);
        }
    }
}
