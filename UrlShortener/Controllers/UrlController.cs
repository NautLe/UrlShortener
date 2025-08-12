using Microsoft.AspNetCore.Mvc;
using UrlShortener.Data;
using UrlShortener.Models;
using System;
using System.Linq;

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

        // POST: api/url/shorten
        [HttpPost("shorten")]
        public IActionResult Shorten([FromBody] string originalUrl)
        {
            if (string.IsNullOrWhiteSpace(originalUrl))
                return BadRequest("URL cannot be empty.");

            if (!Uri.TryCreate(originalUrl, UriKind.Absolute, out Uri? validatedUri) ||
                (validatedUri.Scheme != Uri.UriSchemeHttp && validatedUri.Scheme != Uri.UriSchemeHttps))
                return BadRequest("Invalid URL format. Please enter a valid HTTP or HTTPS URL.");

            // Nếu đã tồn tại → trả về luôn
            var existing = _context.ShortUrls.FirstOrDefault(x => x.OriginalUrl == validatedUri.ToString());
            if (existing != null)
            {
                var existingUrl = $"{Request.Scheme}://{Request.Host}/{existing.ShortCode}";
                return Ok(existingUrl);
            }

            // Lấy 3 ký tự đầu từ domain
            var hostPart = validatedUri.Host.Replace("www.", "").Split('.')[0];
            var shortHost = hostPart.Length > 3 ? hostPart.Substring(0, 3) : hostPart;

            // Lấy 3 ký tự đầu từ mỗi segment trong path
            var pathSegments = validatedUri.Segments
                .Select(s => s.Trim('/'))
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Length > 3 ? s.Substring(0, 3) : s)
                .ToList();

            // Ghép lại thành short code
            var combined = shortHost + string.Join("", pathSegments);
            var shortCode = new string(combined.Where(char.IsLetterOrDigit).ToArray());

            // Nếu trùng → thêm số random
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

            var result = $"{Request.Scheme}://{Request.Host}/{shortCode}";
            return Ok(result);
        }

        // GET: api/url/{shortCode}
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
    }
}
